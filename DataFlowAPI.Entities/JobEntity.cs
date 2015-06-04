using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public class JobEntity:IJobEntity
    {
        private string _JobType { get; set; }
        private DateTime? _TimeStarted { get; set; }
        private DateTime? _TimeEnded { get; set; }
        private double _ElapsedSeconds { get; set; }
        private string _JobStatus { get; set; }
        private string _JobRequestJson { get; set; }
        private string _JobResponseJson { get; set; }
        private string _JobId { get; set; }
        public string JobType
        {
            get
            {
                return _JobType;
            }
            set
            {
                _JobType = value;
            }
        }

        public DateTime? TimeStarted
        {
            get
            {
                return _TimeStarted;
            }
            set
            {
                _TimeStarted = value;
            }
        }

        public DateTime? TimeEnded
        {
            get
            {
                return _TimeEnded;
            }
            set
            {
                _TimeEnded = value;
            }
        }

        public double ElapsedSeconds
        {
            get
            {
                return _ElapsedSeconds;
            }
            set
            {
                _ElapsedSeconds = value;
            }
        }

        public string JobStatus
        {
            get
            {
                return _JobStatus;
            }
            set
            {
                _JobStatus = value;
            }
        }

        public string JobRequestJson
        {
            get
            {
                return _JobRequestJson;
            }
            set
            {
                _JobRequestJson = value;
            }
        }

        public string JobResponseJson
        {
            get
            {
                return _JobResponseJson;
            }
            set
            {
                _JobResponseJson = value;
            }
        }

        public string JobId
        {
            get
            {
                return _JobId;
            }
            set
            {
                _JobId = value;
            }
        }
    }
}
