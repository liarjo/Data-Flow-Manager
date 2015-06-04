using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess
{
    public interface IConfigurationRepo
    {
        //string SubscriptionID { get; }
        //string ManagementCertificateThumbprint { get; }
        string StorageAccountString { get; }
    }
}
