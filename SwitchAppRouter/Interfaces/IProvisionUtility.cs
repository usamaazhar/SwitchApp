using LogicEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwitchAppRouter.Interfaces
{
    interface IProvisionUtility
    {
        Task<bool> CheckIfBackendExists(string backendName);
        Task ProvisionNewBackend(string backendName);
        ILogicEngine GetBackendReference(string backendName);
    }
}
