using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SwitchAppRouter.Business;
using SwitchAppRouter.Interfaces;

namespace SwitchAppRouter.Controllers
{
    public class RoutingController : Controller
    {
        IProvisionUtility m_provisionUtility;
        public RoutingController()
        {
            m_provisionUtility = new ProvisionUtility();
        }

        [Route("ProcessRequest/{clientID}/{payload}")]
        [HttpGet]
        public async Task<IActionResult> Get(string clientID,string payload)
        {
            try
            {
                //for the proof of concept Iam considering the backend name as the payload of the request.
                if (!await m_provisionUtility.CheckIfBackendExists(clientID))
                {
                    await m_provisionUtility.ProvisionNewBackend(clientID);
                }
                var logicEngine = m_provisionUtility.GetBackendReference(clientID);
                await logicEngine.AddToProcessingQueueAsync(payload);
                return Ok("Request has been receieved");
            }
            catch (Exception ex)
            {
                return Ok($"Error : {ex.Message}");
            }
        }

    }
}
