using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public class DeleteDetachedDiskRequest
    {
        public string SubscriptionID { get; set; }
        public string ManagementCertificateThumbprint { get; set; }
        public string DiskLabel { get; set; }
        public bool DeleteFromStorage { get; set; }
    }
}
