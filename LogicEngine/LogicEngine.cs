using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogicEngine.Interfaces;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace LogicEngine
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class LogicEngine : StatefulService, IService, ILogicEngine
    {
        IReliableConcurrentQueue<string> m_processQueue;
        string m_queueName = "ProcessingQueue";
        public LogicEngine(StatefulServiceContext context)
            : base(context)
        {
            
        }

        /// <summary>
        /// This method adds the request to reliable concerrunt queue..
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task AddToProcessingQueueAsync(string payload)
        {
            try
            {
                var m_processQueue = await StateManager.GetOrAddAsync<IReliableConcurrentQueue<string>>(m_queueName);

                using (var tx = StateManager.CreateTransaction())
                {
                    await m_processQueue.EnqueueAsync(tx, payload);

                    await tx.CommitAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// this method keeps on dequeuing the payload from the queue and processes it..
        /// </summary>
        /// <returns></returns>
        private async Task ProcessJobAsync(IReliableConcurrentQueue<string> processQueue)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var result = await processQueue.TryDequeueAsync(tx);

                if (result.HasValue)
                {
                    // Do the work
                }

                await tx.CommitAsync();
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var m_processQueue = await StateManager.GetOrAddAsync<IReliableConcurrentQueue<string>>(m_queueName);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ProcessJobAsync(m_processQueue);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
