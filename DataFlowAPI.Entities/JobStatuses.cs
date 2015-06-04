using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public sealed class JobStatuses
    {
        public const string Undefined = "Undefined";
        public const string Pending = "Pending";
        public const string Executing = "Executing";
        public const string Succeeded = "Succeeded";
        public const string Failed = "Failed";
    }
}
