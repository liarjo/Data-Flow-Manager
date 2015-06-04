using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess
{
    internal class webapiConfigRepo:IConfigurationRepo
    {
        private string _conn;
        public webapiConfigRepo(string connectionstring)
        {
            _conn = connectionstring;
        }
        //public string SubscriptionID
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public string ManagementCertificateThumbprint
        //{
        //    get { throw new NotImplementedException(); }
        //}

        public string StorageAccountString
        {
            get { return _conn; }
        }
    }
}
