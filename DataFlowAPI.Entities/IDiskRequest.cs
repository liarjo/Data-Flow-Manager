using System;
namespace DataFlowAPI.Entities
{
    public interface IDiskRequest
    {
        string DiskLabel { get; set; }
        string ManagementCertificateThumbprint { get; set; }
        string ServiceName { get; set; }
        string SubscriptionID { get; set; }
    }

    public class DiskRequest:IDiskRequest
    {
        private string _DiskLabel;

        public string DiskLabel
        {
            get { return _DiskLabel; }
            set { _DiskLabel = value; }
        }
        private string _ManagementCertificateThumbprint;

        public string ManagementCertificateThumbprint
        {
            get { return _ManagementCertificateThumbprint; }
            set { _ManagementCertificateThumbprint = value; }
        }
        private string _ServiceName;

        public string ServiceName
        {
            get { return _ServiceName; }
            set { _ServiceName = value; }
        }
        private int myVar;

        private string _SubscriptionID;

        public string SubscriptionID
        {
            get { return _SubscriptionID; }
            set { _SubscriptionID = value; }
        }

        
    }
}
