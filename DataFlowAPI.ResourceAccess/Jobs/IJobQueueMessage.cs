using System;
namespace DataFlowAPI.ResourceAccess.Jobs
{
    public interface IJobQueueMessage
    {
        string JobID { get; set; }
        string JobType { get; set; }
    }
}
