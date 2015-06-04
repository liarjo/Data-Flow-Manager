using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess.Jobs
{
    public enum JobRepos
    {
        AzureStorage
    }
    public class JobRepositoryFactory
    {
        public static IJobRepository GetRepo(JobRepos repository, IConfigurationRepo configuration)
        {
            IJobRepository aux = null;
            switch (repository)
            {
                case JobRepos.AzureStorage:
                    aux = new JobAzureStorageRepo(configuration);
                    break;
                default:
                    aux = new JobAzureStorageRepo(configuration);
                    break;
            }
            return aux;
        }
    }
}
