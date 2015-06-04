using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public class AttachDiskRequest : IDiskRequest
    {
        public string SubscriptionID { get; set; }
        public string ManagementCertificateThumbprint { get; set; }
        public string ServiceName { get; set; }
        public string VmName { get; set; }
        public string DiskLabel { get; set; }
        public string DriveLetter { get; set; }
        public string PsUsername { get; set; }
        public string PsPassword { get; set; }
    }
}
