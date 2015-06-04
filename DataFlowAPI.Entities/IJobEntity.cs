using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public interface IJobEntity
    {
         string JobType { get; set; }
         DateTime? TimeStarted { get; set; }
         DateTime? TimeEnded { get; set; }
         double ElapsedSeconds { get; set; }
         string JobStatus { get; set; }
         string JobRequestJson { get; set; }
         string JobResponseJson { get; set; }
         string JobId { get; set; }
    }
}
