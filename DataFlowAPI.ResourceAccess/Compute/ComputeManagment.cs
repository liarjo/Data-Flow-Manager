using DataFlowAPI.Entities;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.Compute;
using Microsoft.WindowsAzure.Management.Compute.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess.Compute
{
    internal class ComputeManagment : IComputeManagment
    {
        private List<string> _ResponseLog;
        private TextWriter _logger;
        private int _numberOfRetries=10;
        private int _retryInterval = 1000;

        private IConfigurationRepo _configuration;
        protected static X509Certificate2 GetCertificateByThumbprint(string thumbprint)
        {
            X509Certificate2 cert = null;

            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            if (certCollection.Count > 0)
            {
                cert = certCollection[0];
            }

            certStore.Close();

            return cert;
        }
        private PSCredential GetPSCredential(string username, string password)
        {
            SecureString securePassword = new SecureString();
            char[] passwordChars = password.ToCharArray();
            foreach (char c in passwordChars)
            {
                securePassword.AppendChar(c);
            }
            PSCredential credential = new PSCredential(username, securePassword);
            return credential;
        }
        protected IEnumerable<PSObject> ExecuteRemotePowerShellCommand(string uri, int port, string username, string password, string psScript)
        {
            System.Collections.ObjectModel.Collection<PSObject> results = null;

            var psCredential = GetPSCredential(username, password);

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(true, uri, port, "", "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", psCredential);
            connectionInfo.SkipCACheck = true;
            connectionInfo.SkipCNCheck = true;
            connectionInfo.OperationTimeout = 1000 * 60 * 60 * 2; // 2 hours

            Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo);
            runspace.Open();
            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = runspace;
                ps.AddScript(psScript, true);
                results = ps.Invoke();
            }
            runspace.Close();

            return results;
        }

        private bool IsPowerShellResultObject(PSObject o)
        {
            if (o != null && o.Properties["Success"] != null && o.Properties["Message"] != null && o.Properties["Success"].Value is System.Boolean && o.Properties["Message"].Value is System.String)
                return true;
            else
                return false;
        }

        public ComputeManagment(IConfigurationRepo configuration)
        {
            _configuration = configuration;
        }
        public IDiskInfo GetDiskStatus(Entities.IDiskRequest diskInfo)
        {
            IDiskInfo aux = new DiskInfo();
            aux.status = DiskSatus.NotExist;

            var credentials = new CertificateCloudCredentials(diskInfo.SubscriptionID, GetCertificateByThumbprint(diskInfo.ManagementCertificateThumbprint));

            var compute = new ComputeManagementClient(credentials);
            var disks = compute.VirtualMachineDisks.ListDisks();

            var currentDisk = disks.Where(d => d.Label == diskInfo.DiskLabel).FirstOrDefault();
            if (currentDisk != null)
            {
                if (currentDisk.UsageDetails != null)
                {

                    aux.status = DiskSatus.Attached;
                    aux.DeploymentName = currentDisk.UsageDetails.DeploymentName;
                    aux.HostedServiceName = currentDisk.UsageDetails.HostedServiceName;
                    aux.RoleName = currentDisk.UsageDetails.RoleName;
                }
                else
                {
                    aux.status = DiskSatus.Created;
                }
            }


            return aux;
        }
        private static bool IsConsoleOut(TextWriter textWriter)
        {
            return object.ReferenceEquals(textWriter, Console.Out);
        }
        protected void Log(IJobEntity job, string message)
        {
            string theMessage = string.Format("{0}\t{1}\t{2}", job.JobId, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), message);
            if (_logger != null)
            {
                Console.WriteLine(theMessage);
                if (IsConsoleOut(_logger))
                {
                    //Unit TEST Console output standar
                    Trace.TraceInformation(theMessage);
                }
                else
                {
                    _logger.WriteLine(theMessage);
                }
            }
            if (_ResponseLog == null)
                _ResponseLog = new List<string>();
            _ResponseLog.Add(theMessage);

        }
        private DeploymentGetResponse CheckVmRole(ComputeManagementClient compute, string serviceName, string hostName)
        {
            DeploymentGetResponse deployment = null;

            for (int i = 0; i < _numberOfRetries; i++)
            {
                deployment = compute.Deployments.GetBySlot(serviceName, DeploymentSlot.Production);

                var vmRoleInstance = deployment.RoleInstances.FirstOrDefault(ri => ri.HostName.ToLower() == hostName.ToLower());
                if (vmRoleInstance != null)
                {
                    Console.WriteLine(vmRoleInstance.InstanceName + " " + vmRoleInstance.InstanceStatus);
                    if (vmRoleInstance.InstanceStatus != "ReadyRole")
                        Thread.Sleep(_retryInterval);
                    else
                        break; ;
                }
                else
                {
                    //Log(job, "VM Role Instance not found");
                    deployment = null;
                    break;
                }
            }
            return deployment;
        }
        
        public List<string> ResponseLog { get { return _ResponseLog; } }
        
        public void CreateAttachedDisk(IJobEntity job)
        {
            var request = JsonConvert.DeserializeObject<CreateAttachedDiskRequest>(job.JobRequestJson);
            var credentials = new CertificateCloudCredentials(request.SubscriptionID, GetCertificateByThumbprint(request.ManagementCertificateThumbprint));
            var compute = new ComputeManagementClient(credentials);
            int powerShellPublicPort = 0;

            // GetDeployment
            DeploymentGetResponse currentDeployment = CheckVmRole(compute, request.ServiceName, request.VmName.ToLower());
            if (currentDeployment == null)
            {
                Log(job, "VM Role Instance not found");
                throw new Exception("VM Role Instance not found");
            }
           
            // VM
            var vm = compute.VirtualMachines.Get(request.ServiceName, currentDeployment.Name, request.VmName);
            var configurationSet = vm.ConfigurationSets.FirstOrDefault();
            if (configurationSet != null)
            {
                var powerShellInputEndpoint = configurationSet.InputEndpoints.FirstOrDefault(ep => ep.LocalPort == 5986);
                if (powerShellInputEndpoint != null && powerShellInputEndpoint.Port.HasValue)
                {
                    powerShellPublicPort = powerShellInputEndpoint.Port.Value;
                }
                else
                {
                    Log(job, "PowerShell input endpoint not found");
                    throw new Exception("PowerShell input endpoint not found"); 
                }
            }
            
            // DISK
            var disks = compute.VirtualMachineDisks.ListDisks();
            if (disks.Any(d => d.Label == request.DiskLabel))
            {
                Log(job, string.Format("Disk label {0} already exists", request.DiskLabel));
                throw new Exception(string.Format("Disk label {0} already exists", request.DiskLabel)); 
            }

            int nextAvailableLun = vm.DataVirtualHardDisks.Count;

            string createDiskPsScript = Properties.Resources.CreateDiskPsScript;

            string vhdFileName = request.DiskLabel.ToLower() + ".vhd";

            VirtualMachineDataDiskCreateParameters dataDiskCreateParameters = new VirtualMachineDataDiskCreateParameters();
            dataDiskCreateParameters.HostCaching = VirtualHardDiskHostCaching.ReadOnly;
            dataDiskCreateParameters.Label = request.DiskLabel;
            dataDiskCreateParameters.LogicalDiskSizeInGB = request.DiskSizeInGB;
            dataDiskCreateParameters.LogicalUnitNumber = nextAvailableLun;
            dataDiskCreateParameters.MediaLinkUri = new Uri(request.DiskStorageContainerUrl + vhdFileName);

            //Execute Creation
            Stopwatch watch = new Stopwatch();
            Log(job, string.Format("Starting to create data disk... ServiceName={0} VM={1} DiskLabel={2} DiskSize={3}", request.ServiceName, request.VmName, request.DiskLabel, request.DiskSizeInGB));
            watch.Start();
            var response = compute.VirtualMachineDisks.CreateDataDisk(request.ServiceName, currentDeployment.Name, request.VmName, dataDiskCreateParameters);
            watch.Stop();
            Log(job, string.Format("Creation of data disk took " + watch.Elapsed.TotalSeconds.ToString("n1") + " seconds"));

            // Get the automatically generated disk name
            disks = compute.VirtualMachineDisks.ListDisks();
            string dataDiskName = disks.FirstOrDefault(d => d.Label == request.DiskLabel).Name;

            createDiskPsScript = createDiskPsScript.Replace("{DRIVELETTER}", request.DriveLetter);
            createDiskPsScript = createDiskPsScript.Replace("{FILESYSTEMLABEL}", request.FileSystemLabel);
            createDiskPsScript = createDiskPsScript.Replace("{ALLOCATIONUNITSIZE}", request.AllocationUnitSize.ToString());
            createDiskPsScript = createDiskPsScript.Replace("{DATADISKLABEL}", request.DiskLabel);
            createDiskPsScript = createDiskPsScript.Replace("{DATADISKNAME}", dataDiskName);

            string cloudServiceDomainName = request.ServiceName + ".cloudapp.net";

            Log(job, string.Format("Starting to create partition, volume, and format via Remote PowerShell... ServiceName={0} VM={1} DriveLetter={2} FileSystemLabel={3} AllocationUnitSize={4}", request.ServiceName, request.VmName, request.DriveLetter, request.FileSystemLabel, request.AllocationUnitSize));
            watch.Restart();
            var results = ExecuteRemotePowerShellCommand(cloudServiceDomainName, powerShellPublicPort, request.PsUsername, request.PsPassword, createDiskPsScript);
            watch.Stop();
            Log(job, string.Format("Create partition, volume, and format took " + watch.Elapsed.TotalSeconds.ToString("n1") + " seconds"));
            
            foreach (PSObject psOut in results)
            {
                if (IsPowerShellResultObject(psOut))
                {
                    bool success = (bool)psOut.Properties["Success"].Value;
                    if (!success)
                        job.JobStatus = JobStatuses.Failed;
                }
                Log(job, "Set drive output: " + psOut.ToString());
            }
        }

        public void CreateAttachedDisk(IJobEntity job, System.IO.TextWriter logger)
        {
            _logger = logger;
             CreateAttachedDisk(job);
        }

        public void GetAttachedDisks(IJobEntity job)
        {
            var request = JsonConvert.DeserializeObject<CreateAttachedDiskRequest>(job.JobRequestJson);
            var credentials = new CertificateCloudCredentials(request.SubscriptionID, GetCertificateByThumbprint(request.ManagementCertificateThumbprint));
            var compute = new ComputeManagementClient(credentials);
            int powerShellPublicPort = 0;

            // GetDeployment
            DeploymentGetResponse currentDeployment = CheckVmRole(compute, request.ServiceName, request.VmName.ToLower());
            if (currentDeployment == null)
            {
                Log(job, "VM Role Instance not found");
                throw new Exception("VM Role Instance not found");
            }

            // VM
            var vm = compute.VirtualMachines.Get(request.ServiceName, currentDeployment.Name, request.VmName);
            var configurationSet = vm.ConfigurationSets.FirstOrDefault();
            if (configurationSet != null)
            {
                var powerShellInputEndpoint = configurationSet.InputEndpoints.FirstOrDefault(ep => ep.LocalPort == 5986);
                if (powerShellInputEndpoint != null && powerShellInputEndpoint.Port.HasValue)
                {
                    powerShellPublicPort = powerShellInputEndpoint.Port.Value;
                }
                else
                {
                    Log(job, "PowerShell input endpoint not found");
                    throw new Exception("PowerShell input endpoint not found");
                }
            }

            //Execute
            Stopwatch watch = new Stopwatch();

            string psScript = Properties.Resources.GetAttachedDriveLettersPsScript;

            string cloudServiceDomainName = request.ServiceName + ".cloudapp.net";

            Log(job, string.Format("Starting to get attached disks via Remote PowerShell... ServiceName={0} VM={1}", request.ServiceName, request.VmName));
            watch.Restart();
            var results = ExecuteRemotePowerShellCommand(cloudServiceDomainName, powerShellPublicPort, request.PsUsername, request.PsPassword, psScript);
            watch.Stop();
            Log(job, string.Format("Getting of attached disks took " + watch.Elapsed.TotalSeconds.ToString("n1") + " seconds"));

            foreach (PSObject psOut in results)
            {
                Log(job, psOut.ToString());
            }
        }

        public void GetAttachedDisks(IJobEntity job, TextWriter logger)
        {
            _logger = logger;
            GetAttachedDisks(job);
        }

        public void DetachDisk(IJobEntity job)
        {
            //throw new NotImplementedException();
            var request = JsonConvert.DeserializeObject<DetachDiskRequest>(job.JobRequestJson);

            var credentials = new CertificateCloudCredentials(request.SubscriptionID, GetCertificateByThumbprint(request.ManagementCertificateThumbprint));
            var compute = new ComputeManagementClient(credentials);

            //GetDeployment
            DeploymentGetResponse currentDeployment=CheckVmRole(compute, request.ServiceName, request.VmName.ToLower());
            if (currentDeployment==null)
            {
                Log(job, "VM Role Instance not found");
                throw new Exception("VM Role Instance not found");
            }


            var vm = compute.VirtualMachines.Get(request.ServiceName, currentDeployment.Name, request.VmName);
            var vmDisk = vm.DataVirtualHardDisks.FirstOrDefault(o => o.Label == request.DiskLabel);
            if (vmDisk == null)
            {
                Log(job, string.Format("Disk {0} is not attached to VM {1}", request.DiskLabel, request.VmName));
                throw new Exception(string.Format("Disk {0} is not attached to VM {1}", request.DiskLabel, request.VmName));
            }
            int lun = vmDisk.LogicalUnitNumber ?? 0;

            //Execute
            Stopwatch watch = new Stopwatch();
            Log(job, string.Format("Starting to detach data disk... ServiceName={0} VM={1} DiskLabel={2}", request.ServiceName, request.VmName, request.DiskLabel));
            watch.Start();
            var response = compute.VirtualMachineDisks.DeleteDataDisk(request.ServiceName, currentDeployment.Name, request.VmName, lun, false);
            watch.Stop();
            Log(job, string.Format("Detach of data disk took " + watch.Elapsed.TotalSeconds.ToString("n1") + " seconds"));
           
            //Wait for detach
            for (int i = 0; i < _retryInterval; i++)
            {
                // Confirm that the detach took place
                vm = compute.VirtualMachines.Get(request.ServiceName, currentDeployment.Name, request.VmName);
                vmDisk = vm.DataVirtualHardDisks.FirstOrDefault(o => o.Label == request.DiskLabel);
                if (vmDisk != null)
                {
                    Log(job, string.Format("Disk {0} is still attached to VM {1}", request.DiskLabel, request.VmName));
                    Thread.Sleep(_retryInterval);
                }
                else
                {
                    break;
                }
            }
            
        }

        public void DetachDisk(IJobEntity job, TextWriter logger)
        {
            _logger = logger;
             DetachDisk(job);
        }

        public void DeleteDetachedDisk(IJobEntity job)
        {
            var request = JsonConvert.DeserializeObject<DeleteDetachedDiskRequest>(job.JobRequestJson);

            var credentials = new CertificateCloudCredentials(request.SubscriptionID, GetCertificateByThumbprint(request.ManagementCertificateThumbprint));
            var compute = new ComputeManagementClient(credentials);

            var disks = compute.VirtualMachineDisks.ListDisks();
            var disk = disks.FirstOrDefault(d => d.Label == request.DiskLabel);
            if (disk != null)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Log(job, string.Format("Starting to delete detached disk DiskLabel={0}", request.DiskLabel));
                var result = compute.VirtualMachineDisks.DeleteDisk(disk.Name, request.DeleteFromStorage);
                Log(job, string.Format("Deletion of detached data disk took " + watch.Elapsed.TotalSeconds.ToString("n1") + " seconds"));
                watch.Stop();
            }
            else
            {
                Log(job, string.Format("Error DeleteDetachedDisk: Disk label does not exist. DiskLabel={0}", request.DiskLabel));
                throw new Exception(string.Format("Error DeleteDetachedDisk: Disk label does not exist. DiskLabel={0}", request.DiskLabel));
            }
           
        }

        public void DeleteDetachedDisk(IJobEntity job, TextWriter logger)
        {
            _logger = logger;
            DeleteDetachedDisk(job);
        }

        public void AttachDisk(IJobEntity job)
        {
            var request = JsonConvert.DeserializeObject<AttachDiskRequest>(job.JobRequestJson);
            var credentials = new CertificateCloudCredentials(request.SubscriptionID, GetCertificateByThumbprint(request.ManagementCertificateThumbprint));
            var compute = new ComputeManagementClient(credentials);

            int powerShellPublicPort = 0;

            //GetDeployment
            DeploymentGetResponse currentDeployment = CheckVmRole(compute, request.ServiceName, request.VmName.ToLower());
            if (currentDeployment == null)
            {
                Log(job, "VM Role Instance not found");
                throw new Exception("VM Role Instance not found");
            }
        
            //VM & disk
            var vm = compute.VirtualMachines.Get(request.ServiceName, currentDeployment.Name, request.VmName);
            var configurationSet = vm.ConfigurationSets.FirstOrDefault();
            if (configurationSet != null)
            {
                var powerShellInputEndpoint = configurationSet.InputEndpoints.FirstOrDefault(ep => ep.LocalPort == 5986);
                if (powerShellInputEndpoint != null && powerShellInputEndpoint.Port.HasValue)
                {
                    powerShellPublicPort = powerShellInputEndpoint.Port.Value;
                }
                else
                {
                    Log(job, "PowerShell input endpoint not found");
                    throw new Exception("PowerShell input endpoint not found");
                }
            }


            var disks = compute.VirtualMachineDisks.ListDisks();
            var disk = disks.FirstOrDefault(d => d.Label == request.DiskLabel);
            if (disk == null)
            {
                Log(job, string.Format("Disk label {0} does not exist", request.DiskLabel));
                throw new Exception(string.Format("Disk label {0} does not exist", request.DiskLabel));
            }

            int nextAvailableLun = vm.DataVirtualHardDisks.Count;

            string attachDiskPsScript = Properties.Resources.AttachDiskPsScript;

            VirtualMachineDataDiskCreateParameters dataDiskCreateParameters = new VirtualMachineDataDiskCreateParameters();
            dataDiskCreateParameters.HostCaching = VirtualHardDiskHostCaching.ReadOnly;
            dataDiskCreateParameters.Name = disk.Name;
            dataDiskCreateParameters.LogicalUnitNumber = nextAvailableLun;
            dataDiskCreateParameters.MediaLinkUri = disk.MediaLinkUri;

            Stopwatch watch = new Stopwatch();

            Log(job, string.Format("Starting to attach data disk... ServiceName={0} VM={1} DiskLabel={2}", request.ServiceName, request.VmName, request.DiskLabel));
            watch.Start();
            var response = compute.VirtualMachineDisks.CreateDataDisk(request.ServiceName, currentDeployment.Name, request.VmName, dataDiskCreateParameters);
            watch.Stop();
            Log(job, string.Format("Attachment of data disk took " + watch.Elapsed.TotalSeconds.ToString("n1") + " seconds"));

            attachDiskPsScript = attachDiskPsScript.Replace("{DRIVELETTER}", request.DriveLetter);

            string cloudServiceDomainName = request.ServiceName + ".cloudapp.net";

            Log(job, string.Format("Starting to set drive letter via Remote PowerShell... ServiceName={0} VM={1} DriveLetter={2}", request.ServiceName, request.VmName, request.PsPassword));
            watch.Restart();
            var results = ExecuteRemotePowerShellCommand(cloudServiceDomainName, powerShellPublicPort, request.PsUsername, request.PsPassword, attachDiskPsScript);
            
            watch.Stop();
            Log(job, string.Format("Set drive letter took " + watch.Elapsed.TotalSeconds.ToString("n1") + " seconds"));
            // [1]: {@{Message=Drive letter M already in use; DiskNumber=0; PartitionNumber=0; Success=False; OldDriveLetter=; NewDriveLetter=}}
            foreach (PSObject psOut in results)
            {
                Log(job,"Set drive output: " +  psOut.ToString());
            }
        }

        public void AttachDisk(IJobEntity job, TextWriter logger)
        {
            _logger = logger;
            AttachDisk(job);
        }


        public void CopyFromBlobToAttachedDisk(IJobEntity job)
        {
            var request = JsonConvert.DeserializeObject<CopyFromBlobToAttachedDiskRequest>(job.JobRequestJson);
            var credentials = new CertificateCloudCredentials(request.SubscriptionID, GetCertificateByThumbprint(request.ManagementCertificateThumbprint));
            var compute = new ComputeManagementClient(credentials);

            int powerShellPublicPort = 0;

            //GetDeployment
            DeploymentGetResponse currentDeployment = CheckVmRole(compute, request.ServiceName, request.VmName.ToLower());
            if (currentDeployment == null)
            {
                Log(job, "VM Role Instance not found");
                throw new Exception("VM Role Instance not found");
            }

            var vm = compute.VirtualMachines.Get(request.ServiceName, currentDeployment.Name, request.VmName);
            var configurationSet = vm.ConfigurationSets.FirstOrDefault();
            if (configurationSet != null)
            {
                var powerShellInputEndpoint = configurationSet.InputEndpoints.FirstOrDefault(ep => ep.LocalPort == 5986);
                if (powerShellInputEndpoint != null && powerShellInputEndpoint.Port.HasValue)
                {
                    powerShellPublicPort = powerShellInputEndpoint.Port.Value;
                }
                else
                {
                    Log(job, "PowerShell input endpoint not found");
                    throw new Exception("PowerShell input endpoint not found");
                }
            }

            Stopwatch watch = new Stopwatch();

            string copyFromBlobPsScript = Properties.Resources.CopyFromBlobPsScript;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("/Source:\"{0}\" ", request.SourceUrl);
            sb.AppendFormat("/Dest:\"{0}\" ", request.DestinationDriveLetterAndPath);
            sb.AppendFormat("/Pattern:\"{0}\" ", request.FileNamePattern);

            // Either the sourceKey or sourceSAS arguments must be provided
            if (!string.IsNullOrWhiteSpace(request.SourceKey))
                sb.AppendFormat("/SourceKey:\"{0}\" ", request.SourceKey);
            else
                sb.AppendFormat("/SourceSAS:\"{0}\" ", request.SourceSAS);

            // Recursive copy of all subfolders and contents that match the pattern
            sb.AppendFormat("/S ");

            // Journal file
            sb.AppendFormat("/Z:\"{0}\" ", @"D:\AzCopy.jnl");

            // Verbose logging to a file
            sb.AppendFormat("/V:\"{0}\" ", @"D:\AzCopy.log");

            // Answer all overwrite prompts with a Yes
            sb.AppendFormat("/Y ");

            string azCopyArguments = sb.ToString();

            copyFromBlobPsScript = copyFromBlobPsScript.Replace("{AZCOPYARGUMENTS}", azCopyArguments);

            string cloudServiceDomainName = request.ServiceName + ".cloudapp.net";

            Log(job, string.Format("Starting to copy from blob via Remote PowerShell... ServiceName={0} VM={1} AzCopyArguments={2}", request.ServiceName, request.VmName, azCopyArguments));
            watch.Restart();
            var results = ExecuteRemotePowerShellCommand(cloudServiceDomainName, powerShellPublicPort, request.PsUsername, request.PsPassword, copyFromBlobPsScript);
            watch.Stop();
            Log(job, string.Format("Copy from blob took " + watch.Elapsed.TotalSeconds.ToString("n1") + " seconds"));
            foreach (PSObject psOut in results)
            {
                Log(job, "Copy from blob took: " + psOut.ToString());
            }
        }

        public void CopyFromBlobToAttachedDisk(IJobEntity job, TextWriter logger)
        {
            _logger = logger;
            CopyFromBlobToAttachedDisk(job);
        }
    }
}
