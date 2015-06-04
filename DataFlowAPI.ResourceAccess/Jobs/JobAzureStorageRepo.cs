using DataFlowAPI.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess.Jobs
{
    internal class JobTableEntity : TableEntity
    {
        public string JobType { get; set; }
        public DateTime? TimeStarted { get; set; }
        public DateTime? TimeEnded { get; set; }
        public double ElapsedSeconds { get; set; }
        public string JobStatus { get; set; }
        public string JobRequestJson { get; set; }
        public string JobResponseJson { get; set; }

        public JobTableEntity(string jobID)
        {
            this.PartitionKey = jobID;
            this.RowKey = jobID;
        }

        public JobTableEntity()
        {
        }

        //public JobTableEntity(IJobEntity jobInfo)
        //{
        //    ElapsedSeconds = jobInfo.ElapsedSeconds;
        //    JobRequestJson = jobInfo.JobRequestJson;
        //    JobResponseJson = jobInfo.JobResponseJson;
        //    JobStatus = jobInfo.JobStatus;
        //    JobType = jobInfo.JobType;
        //    PartitionKey = jobInfo.JobId;
        //    RowKey = jobInfo.JobId;
        //    TimeEnded = jobInfo.TimeEnded;
        //    TimeStarted = jobInfo.TimeStarted;
        //    ETag = "*";
        //}
    }
    public   class JobQueueMessage : DataFlowAPI.ResourceAccess.Jobs.IJobQueueMessage
    {
        public string JobID { get; set; }
        public string JobType { get; set; }
    }
    internal class JobAzureStorageRepo : IJobRepository
    {
        private IConfigurationRepo _repoConfig;
        private CloudTable _jobTable;
        private CloudQueue _jobQueue;
        public JobAzureStorageRepo(IConfigurationRepo repoConfig)
        {
            _repoConfig = repoConfig;
            var storageAccount = CloudStorageAccount.Parse(_repoConfig.StorageAccountString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _jobTable = tableClient.GetTableReference(StorageObjectNames.JobTable);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            _jobQueue = queueClient.GetQueueReference(StorageObjectNames.JobQueue);

            _jobTable.CreateIfNotExists();
            _jobQueue.CreateIfNotExists();
        }
        private JobTableEntity getJobTableEntity(IJobEntity jobInfo)
        {
           JobTableEntity auxJob= new JobTableEntity()
            {
                ElapsedSeconds = jobInfo.ElapsedSeconds,
                JobRequestJson = jobInfo.JobRequestJson,
                JobResponseJson = jobInfo.JobResponseJson,
                JobStatus = jobInfo.JobStatus,
                JobType = jobInfo.JobType,
                PartitionKey = jobInfo.JobId,
                RowKey = jobInfo.JobId,
                TimeEnded = jobInfo.TimeEnded,
                TimeStarted = jobInfo.TimeStarted,
                ETag = "*",
            };
           return auxJob;
        }
        private IJobEntity getIJobEntity(JobTableEntity jobInfo)
        {
            IJobEntity aux = new JobEntity()
            {
                
                ElapsedSeconds=jobInfo.ElapsedSeconds,
                JobRequestJson=jobInfo.JobRequestJson,
                JobResponseJson=jobInfo.JobResponseJson,
                JobStatus=jobInfo.JobStatus,
                JobType=jobInfo.JobType,
                
                TimeEnded=jobInfo.TimeEnded,
                TimeStarted = jobInfo.TimeStarted,
              JobId=jobInfo.PartitionKey

            };
            return aux;
        }
        
        public IJobEntity GetJob(string JobId)
        {
            IJobEntity myJobEntity = null;
            var retrieveResult = _jobTable.Execute(TableOperation.Retrieve<JobTableEntity>(JobId, JobId));
            if (retrieveResult.Result != null)
            {
                var job = (JobTableEntity)retrieveResult.Result;
                myJobEntity = new JobEntity()
                {
                    ElapsedSeconds = job.ElapsedSeconds,
                    JobId = job.PartitionKey,
                    JobRequestJson = job.JobRequestJson,
                    JobResponseJson = job.JobResponseJson,
                    JobStatus = job.JobStatus,
                    JobType = job.JobType,
                    TimeEnded = job.TimeEnded,
                    TimeStarted = job.TimeStarted
                };
            }

            return myJobEntity;
        }
        public void UpdateJobStatus(IJobEntity jobInfo)
        {
            JobTableEntity auxJob = getJobTableEntity(jobInfo);
            _jobTable.Execute(TableOperation.Replace(auxJob));
        }

        public IJobEntity CreateJob(Guid jobId, JobTypesAPI jobType, string jsonRequest)
        {
            IJobEntity aux = null;

            
            var jobMsg = new JobQueueMessage();
            jobMsg.JobID = jobId.ToString();
            jobMsg.JobType = jobType.ToString();

            var job = new JobTableEntity(jobMsg.JobID);
            job.JobType = jobMsg.JobType;
            job.JobStatus = JobStatuses.Pending;
            job.JobRequestJson = jsonRequest;
            _jobTable.Execute(TableOperation.Insert(job));

            _jobQueue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(jobMsg)));

            aux = getIJobEntity(job);

            return aux;
        }
    }
}
