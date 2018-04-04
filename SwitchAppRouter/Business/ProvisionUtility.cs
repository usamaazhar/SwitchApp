using LogicEngine.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using SwitchAppRouter.Interfaces;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitchAppRouter.Business
{
    public class ProvisionUtility : IProvisionUtility
    {
        private string m_appName = FabricRuntime.GetActivationContext().ApplicationName;

        /// <summary>
        /// Checks if a backend with this name exists or not
        /// </summary>
        /// <param name="backendName"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfBackendExists(string backendName)
        {
            Uri serviceName = new Uri($"{m_appName}/{backendName}");
            using (var fabricClient = new FabricClient())
            {
                var serviceList = await fabricClient.QueryManager.GetServiceListAsync(new Uri(m_appName), serviceName);
                if (serviceList.Count > 0)
                    return true;
                return false;
            }
        }



        /// <summary>
        /// Provisions a new backend and name it uniquely according to the name in param
        /// </summary>
        /// <param name="backendName"></param>
        /// <returns></returns>
        public async Task ProvisionNewBackend(string backendName)
        {
            Uri serviceName = new Uri($"{m_appName}/{backendName}");
            var applicationName = new Uri(m_appName);
            var payload = backendName;
            StatefulServiceDescription serviceDescription = new StatefulServiceDescription()
            {
                ApplicationName = applicationName,
                ServiceTypeName = "LogicEngineType",
                ServiceName = serviceName,
                PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription(3, 0, 2),//TODO read from config
                ServicePackageActivationMode = ServicePackageActivationMode.ExclusiveProcess,
                HasPersistedState = true,
                MinReplicaSetSize = 1,
                TargetReplicaSetSize = 1
            };
            //launch service
            try
            {
                using (var fabricClient = new FabricClient())
                {
                    await fabricClient.ServiceManager.CreateServiceAsync(serviceDescription);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ILogicEngine GetBackendReference(string backendName)
        {
            ILogicEngine backendClient =
                ServiceProxy.Create<ILogicEngine>(new Uri($"{m_appName}/{backendName}"),new ServicePartitionKey(0));
            return backendClient;

        }
    }
}
