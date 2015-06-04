using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess
{
    internal class TestConfigRepo:IConfigurationRepo
    {
        public string SubscriptionID
        {
            get { return "4b07ee3a-a9d3-46a7-9640-b3a56b60dc2a"; }
        }

        public string ManagementCertificateThumbprint
        {
            get { return "AC88B63CB91FE9AD0B999FEB6431E946D7544E64"; }
        }

        public string StorageAccountString
        {
            get { return "DefaultEndpointsProtocol=https;AccountName=sonydadcpoc;AccountKey=TofyTThoW4Zv6uJ9DxCjKApI48eLTpOxSGr6/2euGko6YnXJiNQ2rLsU6u63CRJiEhLHoB651LkgIaN4FEItIA=="; }
        }
    }
}
