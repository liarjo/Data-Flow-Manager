using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using DataFlowAPI.Entities;
using Microsoft.WindowsAzure.Storage.Table;
using DataFlowAPI.ResourceAccess.Jobs;
using DataFlowAPI.ResourceAccess;
using System.Configuration;
using Newtonsoft.Json;
using DataFlowAPI.ResourceAccess.Compute;

namespace DataFlowAPI.ControlJob
{
    public class Functions
    {
               
        public static void ProcessJobQueueMessage([QueueTrigger(StorageObjectNames.JobQueue)] JobQueueMessage jobMsg, [Table(StorageObjectNames.JobTable)] CloudTable jobTable, TextWriter logger)
        {
            string connstr = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;
            IConfigurationRepo config = ConfigurationRepoFactory.GetConfiguration(ConfigurationRepos.webapi, connstr);
            IJobRepository repo = JobRepositoryFactory.GetRepo(JobRepos.AzureStorage, config);
            IJobEntity currentJob= repo.GetJob(jobMsg.JobID);
            IComputeManagment compute = ComputeManagmentFactory.GetComputeManagment(config);


            if (currentJob.JobStatus != JobStatuses.Pending)
            {
                logger.WriteLine("Jobid {0} is in status {1}, not processed again.", currentJob.JobId, currentJob.JobStatus);
                return;
            }
            currentJob.JobStatus = JobStatuses.Executing;
            currentJob.TimeStarted = DateTime.UtcNow;
            repo.UpdateJobStatus(currentJob);

            try
            {
                JobTypesAPI currentType = (JobTypesAPI)Enum.Parse(typeof(JobTypesAPI), currentJob.JobType, true);
                switch (currentType)
                {
                    case JobTypesAPI.CreateAttachedDisk:
                        compute.CreateAttachedDisk(currentJob, logger);
                        currentJob.JobResponseJson = JsonConvert.SerializeObject(compute.ResponseLog);
                        break;
                    case JobTypesAPI.AttachDisk:
                        compute.AttachDisk(currentJob, logger);
                        currentJob.JobResponseJson = JsonConvert.SerializeObject(compute.ResponseLog);
                        break;
                    case JobTypesAPI.DetachDisk:
                        compute.DetachDisk(currentJob, logger);
                        currentJob.JobResponseJson = JsonConvert.SerializeObject(compute.ResponseLog);
                        break;
                    case JobTypesAPI.CopyFromBlobToAttachedDisk:
                        compute.CopyFromBlobToAttachedDisk(currentJob, logger);
                        currentJob.JobResponseJson = JsonConvert.SerializeObject(compute.ResponseLog);
                        break;
                    case JobTypesAPI.DeleteDetachedDisk:
                        compute.DeleteDetachedDisk(currentJob, logger);
                        currentJob.JobResponseJson = JsonConvert.SerializeObject(compute.ResponseLog);
                        break;
                    case JobTypesAPI.GetAttachedDisks:
                        compute.GetAttachedDisks(currentJob, logger);
                        currentJob.JobResponseJson = JsonConvert.SerializeObject(compute.ResponseLog);
                        break;
                    default:
                       
                        break;
                }
                currentJob.JobStatus = JobStatuses.Succeeded;
            }
            catch (Exception ex)
            {
                currentJob.JobStatus = JobStatuses.Failed;
                currentJob.JobResponseJson = JsonConvert.SerializeObject(new { ErrorMessage = ex.Message });
            }

            currentJob.TimeEnded = DateTime.UtcNow;
            currentJob.ElapsedSeconds = ((TimeSpan)(currentJob.TimeEnded - currentJob.TimeStarted)).TotalSeconds;
            repo.UpdateJobStatus(currentJob);

            
        }
    }
}
