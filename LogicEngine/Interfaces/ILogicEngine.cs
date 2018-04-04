using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogicEngine.Interfaces
{
    public interface ILogicEngine : IService
    {
        Task AddToProcessingQueueAsync(string payload);
    }
}
