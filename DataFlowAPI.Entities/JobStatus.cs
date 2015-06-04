using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public class JobStatusInfo : IJobStatusInfo
    {
        private string _JobId;

        public string JobId
        {
            get { return _JobId; }
            set { _JobId = value; }
        }
        
        private string  _JobResponseJson;

        public string  JobResponseJson
        {
            get { return _JobResponseJson; }
            set { _JobResponseJson = value; }
        }
        
        private string _JobStatus;

        public string JobStatus
        {
            get { return _JobStatus; }
            set { _JobStatus = value; }
        }

        private string _JobType;

        public string JobType
        {
            get { return _JobType; }
            set { _JobType = value; }
        }

        private DateTime? _TimeStarted;

        public DateTime? TimeStarted
        {
            get { return _TimeStarted; }
            set { _TimeStarted = value; }
        }
        private DateTime? _TimeEnded;

        public DateTime? TimeEnded
        {
            get { return _TimeEnded; }
            set { _TimeEnded = value; }
        }
        
    }
}
