using DataFlowAPI.Entities;
using DataFlowAPI.ResourceAccess.Jobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Tests
{
    class UtilTest
    {
        public static string _PsPassword
        {
            get
            { return ConfigurationManager.AppSettings["PsPassword"]; }
        }
        public static string _PsUsername
        {
            get
            { return ConfigurationManager.AppSettings["PsUsername"]; }
        }
        public static string _DriveLetter
        {
            get
            { return ConfigurationManager.AppSettings["DriveLetter"]; }
        }
        public static string _DiskStorageContainerUrl
        {
            get
            { return ConfigurationManager.AppSettings["DiskStorageContainerUrl"]; }
        }
        public static string _VmName
        {
            get
            { return ConfigurationManager.AppSettings["VmName"]; }
        }
        public static string _ServiceName
        {
            get
            { return ConfigurationManager.AppSettings["ServiceName"]; }
        }
        public static string _DiskLabel
        {
            get
            { return ConfigurationManager.AppSettings["DiskLabel"]; }
        }
         public static string _ManagementCertificateThumbprint
        {
            get
            { return ConfigurationManager.AppSettings["ManagementCertificateThumbprint"]; }
        }
        public static string _SubscriptionID
        {
            get
            { return ConfigurationManager.AppSettings["SubscriptionID"]; }
        }
      
        public static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
        public static CloudTable GetJobTable(string repoConfig)
        {
           
            var storageAccount = CloudStorageAccount.Parse(repoConfig);
            var tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference(StorageObjectNames.JobTable);
          
        }
        public static CloudQueueMessage getMessageJobUtilTest(string repoConfig)
        {
            var storageAccount = CloudStorageAccount.Parse(repoConfig);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            var _jobQueue = queueClient.GetQueueReference(StorageObjectNames.JobQueue);
            _jobQueue.CreateIfNotExists();
            CloudQueueMessage peekedMessage = _jobQueue.PeekMessage();
            return peekedMessage;
        }
        public static void DeleteAllMessage( string repoConfig)
        {

            var storageAccount = CloudStorageAccount.Parse(repoConfig);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            var _jobQueue = queueClient.GetQueueReference(StorageObjectNames.JobQueue);
            _jobQueue.CreateIfNotExists();
            _jobQueue.Clear();


        }
        public static CreateAttachedDiskRequest getCreateAttachedDiskRequest()
        {
            return new CreateAttachedDiskRequest
            {
                SubscriptionID = _SubscriptionID,
                ManagementCertificateThumbprint = _ManagementCertificateThumbprint,
                ServiceName = _ServiceName,
                VmName = _VmName,
                DiskLabel = _DiskLabel,
                DiskSizeInGB = 5,
                DiskStorageContainerUrl = _DiskStorageContainerUrl,
                DriveLetter = _DriveLetter,
                FileSystemLabel = "filesystem_100",
                AllocationUnitSize = 65536,
                PsUsername = _PsUsername,
                PsPassword = _PsPassword
            };
        }
        public static IDiskRequest GetDiskRequest()
        {
            return new DiskRequest()
            {
                DiskLabel = UtilTest._DiskLabel,
                ManagementCertificateThumbprint = UtilTest._ManagementCertificateThumbprint,
                SubscriptionID = UtilTest._SubscriptionID,
                ServiceName = UtilTest._ServiceName,
            };
        }
        public static DetachDiskRequest getDetachDiskRequest()
        {
            return new DetachDiskRequest()
            {
                DiskLabel = _DiskLabel,
                ManagementCertificateThumbprint = _ManagementCertificateThumbprint,
                ServiceName = _ServiceName,
                SubscriptionID = _SubscriptionID,
                VmName = _VmName
            };
        }
        public static DeleteDetachedDiskRequest getDeleteDetachedDiskRequest()
        {
            return new DeleteDetachedDiskRequest
            {
                SubscriptionID = _SubscriptionID,
                ManagementCertificateThumbprint = _ManagementCertificateThumbprint,
                DiskLabel = _DiskLabel,
                DeleteFromStorage = true
            };
        }
        public static AttachDiskRequest getAttachDiskRequest()
        {
            return new AttachDiskRequest()
            {
                DiskLabel = _DiskLabel,
                DriveLetter = _DriveLetter,
                ManagementCertificateThumbprint = _ManagementCertificateThumbprint,
                PsUsername = _PsUsername,
                PsPassword = _PsPassword,

                SubscriptionID = _SubscriptionID,
                ServiceName = _ServiceName,
                VmName = _VmName
            };
        }
        public static CopyFromBlobToAttachedDiskRequest getCopyFromBlobToAttachedDiskRequest()
        {
            return new CopyFromBlobToAttachedDiskRequest
            {
                DestinationDriveLetterAndPath = "M:\\Data4\\",
                FileNamePattern = "100m",

                SubscriptionID = _SubscriptionID,
                ManagementCertificateThumbprint = _ManagementCertificateThumbprint,
                PsUsername = _PsUsername,
                PsPassword = _PsPassword,

                SourceUrl = "https://avvs2015rc.blob.core.windows.net/test",
                SourceKey = "FMRiuZuFmLIv+KuVFM8wNSqOH1VUzt0sKeOBAoG8fXT0v8NX/7a69brKuAul9wBmTYXSRmUUezmPiQRChHSAXQ==",
                SourceSAS = "",


                ServiceName = _ServiceName,
                VmName = _VmName
            };
        }
    }
}
