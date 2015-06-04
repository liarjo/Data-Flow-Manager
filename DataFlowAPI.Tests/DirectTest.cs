using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using DataFlowAPI.ControlAPI.Controllers;
using DataFlowAPI.Entities;
using System.Web.Http.Results;
using DataFlowAPI.ResourceAccess.Jobs;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;

namespace DataFlowAPI.Tests
{
    [TestClass]
    public class DirectTest
    {
        private string storageConn;
        private IDiskInfo diskStatusInfo;
        private void Init()
        {
            storageConn = UtilTest.GetConnectionString("AzureWebJobsStorage");

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        }
        
        [TestMethod]
        public void DiskStatus()
        {
            //ARRANGE
            string xMessage = "ok";
            Init();
            var controller = new ManageDataDiskController(storageConn);
            var xdisk =(DiskRequest) UtilTest.GetDiskRequest();
            //ACT
            try
            {

                IDiskInfo response = (controller.GetDiskStatus(xdisk) as OkNegotiatedContentResult<IDiskInfo>).Content;
                Assert.AreNotEqual(response, null);

                diskStatusInfo = response;
                Trace.TraceInformation("DiskStatus:" + response.status.ToString());
            }
            catch (Exception X)
            {
                xMessage = X.Message;
                Trace.TraceError(xMessage);
            }

            Assert.AreEqual(xMessage, "ok");
        }
        [TestMethod]
        public void GetJobStatus()
        {
            //ARRANGE
            string xMessage = "ok";
            Init();
            var controller = new ManageDataDiskController(storageConn);
            //ACT
            try
            {

                IJobStatusInfo response = (controller.GetJobStatus("e621682b-b567-41f2-8f1e-8c17ed1c4711") as OkNegotiatedContentResult<IJobStatusInfo>).Content;
            }
            catch (Exception X)
            {
                xMessage = X.Message;
                Trace.TraceError(xMessage);
            }

            Assert.AreEqual(xMessage, "ok");
        }
        [TestMethod]
        public void CreateAttachedDisk()
        {
            //ARRANGE
            string xMessage = "ok";
            Init();
            UtilTest.DeleteAllMessage(storageConn);
            CreateAttachedDiskRequest myRequestData = UtilTest.getCreateAttachedDiskRequest();
            var controller = new ManageDataDiskController(storageConn);
            //WEBJOB
            JobQueueMessage jobMsg;
            CloudTable jobTable;
            TextWriter logger = Console.Out;
            //ACT
            try
            {
                var response = controller.CreateAttachedDisk(myRequestData);
                OkNegotiatedContentResult<IJobResponse> x = response as OkNegotiatedContentResult<IJobResponse>;
                string jobId = x.Content.JobID;

                jobTable = UtilTest.GetJobTable(storageConn);
                var msg=UtilTest.getMessageJobUtilTest(storageConn);

                if (msg != null)
                {
                    jobMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<JobQueueMessage>(msg.AsString);
                }
                else
                {
                    throw new Exception("no message");
                }
                DataFlowAPI.ControlJob.Functions.ProcessJobQueueMessage(jobMsg, jobTable, logger);
                //check job Status

                IJobStatusInfo StatusResponse = (controller.GetJobStatus(jobMsg.JobID) as OkNegotiatedContentResult<IJobStatusInfo>).Content;
                
                //assert
                Assert.AreEqual(StatusResponse.JobStatus, "Succeeded");

            }
            catch (Exception X)
            {
                xMessage = X.Message;
                Trace.TraceError(xMessage);
            }

            Assert.AreEqual(xMessage, "ok");

        }
        [TestMethod]
        public void DetachDisk()
        {
            //ARRANGE
            string xMessage = "ok";
            Init();
            UtilTest.DeleteAllMessage(storageConn);
            DetachDiskRequest myRequestData = UtilTest.getDetachDiskRequest();
            var controller = new ManageDataDiskController(storageConn);
            //WEBJOB
            JobQueueMessage jobMsg;
            CloudTable jobTable;
            TextWriter logger = Console.Out;
            //ACT
            try
            {
                var response = controller.DetachDisk(myRequestData);
                OkNegotiatedContentResult<IJobResponse> x = response as OkNegotiatedContentResult<IJobResponse>;
                string jobId = x.Content.JobID;

                jobTable = UtilTest.GetJobTable(storageConn);
                var msg = UtilTest.getMessageJobUtilTest(storageConn);

                if (msg != null)
                {
                    jobMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<JobQueueMessage>(msg.AsString);
                }
                else
                {
                    throw new Exception("no message");
                }
                DataFlowAPI.ControlJob.Functions.ProcessJobQueueMessage(jobMsg, jobTable, logger);

                IJobStatusInfo StatusResponse = (controller.GetJobStatus(jobMsg.JobID) as OkNegotiatedContentResult<IJobStatusInfo>).Content;

                //assert
                Assert.AreEqual(StatusResponse.JobStatus, "Succeeded");

            }
            catch (Exception X)
            {
                xMessage = X.Message;
                Trace.TraceError(xMessage);
            }

            Assert.AreEqual(xMessage, "ok");

        }
        [TestMethod]
        public void DeleteDetachedDisk()
        {
            //ARRANGE
            Init();
            string xMessage = "ok";
            UtilTest.DeleteAllMessage(storageConn);
           
            DeleteDetachedDiskRequest myRequestData = UtilTest.getDeleteDetachedDiskRequest();
            var controller = new ManageDataDiskController(storageConn);
            //WEBJOB
            JobQueueMessage jobMsg;
            CloudTable jobTable;
            TextWriter logger = Console.Out;
            //ACT
            try
            {
                var response = controller.DeleteDetachedDisk(myRequestData);
                OkNegotiatedContentResult<IJobResponse> x = response as OkNegotiatedContentResult<IJobResponse>;
                string jobId = x.Content.JobID;

                jobTable = UtilTest.GetJobTable(storageConn);
                var msg = UtilTest.getMessageJobUtilTest(storageConn);

                if (msg != null)
                {
                    jobMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<JobQueueMessage>(msg.AsString);
                }
                else
                {
                    throw new Exception("no message");
                }
                DataFlowAPI.ControlJob.Functions.ProcessJobQueueMessage(jobMsg, jobTable, logger);

                IJobStatusInfo StatusResponse = (controller.GetJobStatus(jobMsg.JobID) as OkNegotiatedContentResult<IJobStatusInfo>).Content;

                //assert
                Assert.AreEqual(StatusResponse.JobStatus, "Succeeded");

            }
            catch (Exception X)
            {
                xMessage = X.Message;
                Trace.TraceError(xMessage);
            }

            Assert.AreEqual(xMessage, "ok");

        }
        [TestMethod]
        public void AttachDisk()
        {
            //ARRANGE
            Init();
            string xMessage = "ok";
            UtilTest.DeleteAllMessage(storageConn);

            AttachDiskRequest myRequestData = UtilTest.getAttachDiskRequest();
            var controller = new ManageDataDiskController(storageConn);
            //WEBJOB
            JobQueueMessage jobMsg;
            CloudTable jobTable;
            TextWriter logger = Console.Out;
            //ACT
            try
            {
                var response = controller.AttachDisk(myRequestData);
                OkNegotiatedContentResult<IJobResponse> x = response as OkNegotiatedContentResult<IJobResponse>;
                string jobId = x.Content.JobID;

                jobTable = UtilTest.GetJobTable(storageConn);
                var msg = UtilTest.getMessageJobUtilTest(storageConn);

                if (msg != null)
                {
                    jobMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<JobQueueMessage>(msg.AsString);
                }
                else
                {
                    throw new Exception("no message");
                }
                DataFlowAPI.ControlJob.Functions.ProcessJobQueueMessage(jobMsg, jobTable, logger);

                DataFlowAPI.ControlJob.Functions.ProcessJobQueueMessage(jobMsg, jobTable, logger);

                IJobStatusInfo StatusResponse = (controller.GetJobStatus(jobMsg.JobID) as OkNegotiatedContentResult<IJobStatusInfo>).Content;

                //assert
                Assert.AreEqual(StatusResponse.JobStatus, "Succeeded");

            }
            catch (Exception X)
            {
                xMessage = X.Message;
                Trace.TraceError(xMessage);
            }

            Assert.AreEqual(xMessage, "ok");

        }
        [TestMethod]
        public void CopyFromBlobToAttachedDisk()
        {
            //ARRANGE
            Init();
            string xMessage = "ok";
            UtilTest.DeleteAllMessage(storageConn);

            CopyFromBlobToAttachedDiskRequest myRequestData = UtilTest.getCopyFromBlobToAttachedDiskRequest();
            var controller = new ManageDataDiskController(storageConn);
            //WEBJOB
            JobQueueMessage jobMsg;
            CloudTable jobTable;
            TextWriter logger = Console.Out;
            //ACT
            try
            {
                var response = controller.CopyFromBlobToAttachedDisk(myRequestData);
                OkNegotiatedContentResult<IJobResponse> x = response as OkNegotiatedContentResult<IJobResponse>;
                string jobId = x.Content.JobID;

                jobTable = UtilTest.GetJobTable(storageConn);
                var msg = UtilTest.getMessageJobUtilTest(storageConn);

                if (msg != null)
                {
                    jobMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<JobQueueMessage>(msg.AsString);
                }
                else
                {
                    throw new Exception("no message");
                }
                DataFlowAPI.ControlJob.Functions.ProcessJobQueueMessage(jobMsg, jobTable, logger);

                //UtilTest.DeleteMessage(msg,storageConn);

            }
            catch (Exception X)
            {
                xMessage = X.Message;
                Trace.TraceError(xMessage);
            }

            Assert.AreEqual(xMessage, "ok");
        }
        [TestMethod]
        public void CreateDettachDelete()
        {
            //Disk not exist
            DiskStatus();
            Assert.AreEqual(diskStatusInfo.status, DiskSatus.NotExist);

            CreateAttachedDisk();
            DetachDisk();
            do
            {
                DiskStatus();
                System.Threading.Thread.Sleep(5000); 
                
            } while (diskStatusInfo.status!=DiskSatus.Created);
            DeleteDetachedDisk();
        }
    }
}