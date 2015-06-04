using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public class JobResponse:IJobResponse
    {
        private string _JobID;
        public string JobID
        {
            get
            {
                return _JobID;
            }
            set
            {
                _JobID = value;
            }
        }
    }
}
