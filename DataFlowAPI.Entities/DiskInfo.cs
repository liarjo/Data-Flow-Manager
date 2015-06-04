using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.Entities
{
    public class DiskInfo : IDiskInfo
    {
        private DiskSatus _status;

        public DiskSatus status
        {
            get { return _status; }
            set { _status = value; }
        }
        private string _DeploymentName;

        public string DeploymentName
        {
            get { return _DeploymentName; }
            set { _DeploymentName = value; }
        }
        private string _HostedServiceName;

        public string HostedServiceName
        {
            get { return _HostedServiceName; }
            set { _HostedServiceName = value; }
        }
        private string _RoleName;

        public string RoleName
        {
            get { return _RoleName; }
            set { _RoleName = value; }
        }


    }
}
