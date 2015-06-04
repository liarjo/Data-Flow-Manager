using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  DataFlowAPI.Entities
{
    public class CopyFromBlobToAttachedDiskRequest
    {
        public string SubscriptionID { get; set; }
        public string ManagementCertificateThumbprint { get; set; }
        public string ServiceName { get; set; }
        public string VmName { get; set; }
        public string SourceUrl { get; set; }
        public string SourceKey { get; set; }
        public string SourceSAS { get; set; }
        public string FileNamePattern { get; set; }
        public string DestinationDriveLetterAndPath { get; set; }
        public string PsUsername { get; set; }
        public string PsPassword { get; set; }
    }
}
