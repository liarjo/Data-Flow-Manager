using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess
{
    public enum ConfigurationRepos
    {
        UnitTest,webapi
    }
    public class ConfigurationRepoFactory
    {
        public static IConfigurationRepo GetConfiguration(ConfigurationRepos repoID)
        {

            return GetConfiguration(repoID, null);
        }
        public static IConfigurationRepo GetConfiguration(ConfigurationRepos repoID, string storageAccountConn)
        {
            IConfigurationRepo aux = null;
            switch (repoID)
            {
                case ConfigurationRepos.UnitTest:
                    aux = new TestConfigRepo();
                    break;
                case ConfigurationRepos.webapi:
                    if (storageAccountConn == null)
                        throw new Exception("WebApi must declare explicit Conneciton");
                    aux = new webapiConfigRepo(storageAccountConn);
                    break;
                default:
                    
                    break;
            }
            return aux;
        }
    }
}
