using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess.Compute
{
    public class ComputeManagmentFactory
    {
        public static IComputeManagment GetComputeManagment(IConfigurationRepo configuration)
        {
            return new ComputeManagment(configuration);
        }
    }
}
