using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public enum DiskSatus
    {
        NotExist, Created, Attached
    }
    public interface IDiskInfo
    {
        DiskSatus status { get; set; }
        string DeploymentName { get; set; }
        string HostedServiceName { get; set; }
        string RoleName { get; set; }
    }
}
