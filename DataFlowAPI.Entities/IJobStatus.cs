using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public interface IJobStatusInfo
    {
         string JobStatus { get; set; }
         string JobType { get; set; }
         DateTime? TimeStarted { get; set; }
         DateTime? TimeEnded { get; set; }
         string JobResponseJson { get; set; }
         string JobId { get; set; }
    }
}
