using DataFlowAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess.Jobs
{
    public interface IJobRepository
    {
        IJobEntity GetJob(string JobId);
        void UpdateJobStatus(IJobEntity jobInfo);
        IJobEntity CreateJob(Guid jobId, JobTypesAPI jobType, string jsonRequest);
    }
}
