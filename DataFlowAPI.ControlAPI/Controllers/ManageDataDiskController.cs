using DataFlowAPI.Entities;
using DataFlowAPI.ResourceAccess;
using DataFlowAPI.ResourceAccess.Compute;
using DataFlowAPI.ResourceAccess.Jobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DataFlowAPI.ControlAPI.Controllers
{
    [RoutePrefix("api/ManageDataDisk")]
    public class ManageDataDiskController : ApiController
    {
        private void InitializeStorage(string storageAccountString)
        {
            myConfig = ConfigurationRepoFactory.GetConfiguration(ConfigurationRepos.webapi, storageAccountString);
        }
        public ManageDataDiskController()
        {
            InitializeStorage(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
        }
        public ManageDataDiskController(string storageAccountString)
        {
            InitializeStorage(storageAccountString);
        }
        private IConfigurationRepo myConfig;
        private IHttpActionResult StartJob(JobTypesAPI type,string jsonRequest)
        {
            IJobRepository myRepo = JobRepositoryFactory.GetRepo(JobRepos.AzureStorage, myConfig);
            try
            {
                IJobEntity job = myRepo.CreateJob(Guid.NewGuid(), type, jsonRequest);
                IJobResponse response = new JobResponse();
                response.JobID = job.JobId;
                return Ok(response);
            }
            catch
            {
                string friendlyMessage = string.Format("Error in Create Job {1} at {0}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),type.ToString());
                return BadRequest(friendlyMessage);
            }
        }
        [HttpPost]
        [ActionName("DiskStatus")]
        public IHttpActionResult GetDiskStatus(DiskRequest diskInfo)
        {
            IComputeManagment compute = ComputeManagmentFactory.GetComputeManagment(myConfig);
            try
            {
               var  data= compute.GetDiskStatus(diskInfo);
               return Ok(data);
            }
            catch (Exception ex)
            {

                var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(ex.Message),
                        ReasonPhrase = "Error in API"
                    };
                throw new HttpResponseException(resp); 
            }
            
        }
        [HttpGet]
        [ActionName("GetJobStatus")]
        public IHttpActionResult GetJobStatus(string jobID)
        {

            IJobRepository myJobRepo = JobRepositoryFactory.GetRepo(JobRepos.AzureStorage, myConfig);
            var jobinfo = myJobRepo.GetJob(jobID);
            if (jobinfo == null)
                return NotFound();
       
            IJobStatusInfo response = new JobStatusInfo()
            {
                JobId=jobinfo.JobId,
                JobResponseJson=jobinfo.JobResponseJson,
                JobStatus = jobinfo.JobStatus,
                JobType = jobinfo.JobType,
                TimeStarted = jobinfo.TimeStarted,
                TimeEnded = jobinfo.TimeEnded
            };
            return Ok(response);
        }
        [HttpPost]
        [ActionName("CreateAttachedDisk")]
        public IHttpActionResult CreateAttachedDisk(CreateAttachedDiskRequest request)
        {
            IJobRepository myRepo = JobRepositoryFactory.GetRepo(JobRepos.AzureStorage, myConfig);
            try
            {
                IJobEntity job = myRepo.CreateJob(Guid.NewGuid(), JobTypesAPI.CreateAttachedDisk,JsonConvert.SerializeObject(request) );
                IJobResponse response = new JobResponse();
                response.JobID = job.JobId;
                return Ok(response);
            }
            catch
            {
                string friendlyMessage = string.Format("Error in CreateAttachedDisk at {0}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                return BadRequest(friendlyMessage);
            }
        }
        [HttpPost]
        [ActionName("DetachDisk")]
        public IHttpActionResult DetachDisk(DetachDiskRequest request)
        {
            return StartJob(JobTypesAPI.DetachDisk, JsonConvert.SerializeObject(request));
        }
        [HttpPost]
        [ActionName("DeleteDetachedDisk")]
        public IHttpActionResult DeleteDetachedDisk(DeleteDetachedDiskRequest request)
        {
            return StartJob(JobTypesAPI.DeleteDetachedDisk, JsonConvert.SerializeObject(request));
        }
        [HttpPost]
        [ActionName("AttachDisk")]
        public IHttpActionResult AttachDisk(AttachDiskRequest request)
        {
            return StartJob(JobTypesAPI.AttachDisk, JsonConvert.SerializeObject(request));
        }
        [HttpPost]
        [ActionName("CopyFromBlobToAttachedDisk")]
        public IHttpActionResult CopyFromBlobToAttachedDisk(CopyFromBlobToAttachedDiskRequest request)
        {
            return StartJob(JobTypesAPI.CopyFromBlobToAttachedDisk, JsonConvert.SerializeObject(request));
        }
        [HttpPost]
        [ActionName("GetAttachedDisks")]
        public IHttpActionResult GetAttachedDisks(GetAttachedDisksRequest request)
        {
            return StartJob(JobTypesAPI.GetAttachedDisks, JsonConvert.SerializeObject(request));
        }
    }
}
