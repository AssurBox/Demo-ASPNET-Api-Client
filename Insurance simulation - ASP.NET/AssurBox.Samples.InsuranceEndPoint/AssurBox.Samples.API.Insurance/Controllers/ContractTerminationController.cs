using AssurBox.SDK;
using Hangfire;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AssurBox.Samples.API.Insurance.Controllers
{
    public class ContractTerminationController : ApiController
    {


        //http://docs.hangfire.io/en/latest/background-methods/writing-unit-tests.html
        private readonly IBackgroundJobClient _jobClient;
        public ContractTerminationController() : this(new BackgroundJobClient())
        {
        }

        public ContractTerminationController(IBackgroundJobClient jobClient)
        {
            _jobClient = jobClient;
        }

        /// <summary>
        /// AssurBox is going to post notification on this endpoint (if configured)
        /// AssurBox expects an HTTP 200 response
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// /!\ you need to configure the webhook url in Assurbox 
        /// <see href="http://assurbox.net/developers-insurances-green-card-request-scenario-webhook-happy-flow/"/> 
        /// 
        /// ici on va récupérer la demande qui vient d'AssurBox, la sauver et confirmer qu'on l'a bien reçue.
        /// Si la réception du message n'est pas confirmée, AssurBox va tenter de ré-envoyer le message pendant un temps
        /// et si après un certain nombre de tentatives, la réception n'est toujours pas confirmée, il enverra un email "fallback" avec la demande.
        /// </remarks>
        [Route("webhook/contracttermination")]
        [HttpPost]
        public HttpResponseMessage Post(SDK.WebHooks.AssurBoxWebHook value)
        {
            if (value == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Parameter can't be null");
            }

            Logger.Log($"ContractTermination.Post - Debut {value.CorrelationId}");

            string key = $"{value.CorrelationId}|{value.MessageId}";

            // Save the notification for later processing

            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                ctx.CarGreenCardRequests.Add(new DAL.CarGreenCardRequest // todo: change CarGreenCardRequest for "AssurBoxRequest" (with a type)
                {
                    RequestDate = DateTime.UtcNow,
                    RequestId = key,
                    RawRequest = JsonConvert.SerializeObject(value, Formatting.None),
                });
                ctx.SaveChanges();
            }

            // Creates a job that will be executed as soon as possible
            _jobClient.Enqueue(() => ExecuteJob(key));

            Logger.Log("ContractTermination.Post - Fin");

            // Don't forget to notify AssurBox that the notication is received
            return Request.CreateResponse(HttpStatusCode.OK, value.CorrelationId);
        }

        /// <summary>
        /// Ici on va traiter la demande que l'on a sauvegardée 
        /// on récupère les infos
        /// on crée une réponse
        /// et on envoie la réponse à AssurBox via l'api
        /// </summary>
        /// <param name="key"></param>
        public void ExecuteJob(string key)
        {
            Logger.Log($"ContractTermination.ExecuteJob - Debut {key}");

            // recherche dans la table 
            SDK.WebHooks.AssurBoxWebHook assurBoxNotification;
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                var request = ctx.CarGreenCardRequests.AsNoTracking().FirstOrDefault(x => x.RequestId == key);
                assurBoxNotification = JsonConvert.DeserializeObject<SDK.WebHooks.AssurBoxWebHook>(request.RawRequest);
            }


            // 1. retrieve a token to access the AssurBox Api
            var tokenInfo = AssurBoxSecurity.GetAssurBoxSecurityToken();

            // 2. use the client
            SDK.Clients.ContractTerminationClient client = new SDK.Clients.ContractTerminationClient(new AssurBoxClientOptions { Host = Config.AssurBoxApiBaseURL, ApiKey = tokenInfo });

            // 3. using the authentified client, retrieve the details of the  request 
            //    full detail about the customer, car information, ...
            //var requestDetails = client.(assurBoxNotification.CorrelationId).Result;

            SDK.DTO.Contract.ContractTerminationInfo requestinfo = client.GetContractTerminationRequestAsync(assurBoxNotification.CorrelationId).Result;

            // 4. do something with the request in your information system

            // ...

            // 5. send a response to the requester
            string acceptation = GetAcceptation(requestinfo.Request.Risk);
            var resp = client.SendContractTerminationResponseAsync(new SDK.DTO.Contract.ContractTerminationResponse
            {
                CorrelationId = assurBoxNotification.CorrelationId,
                MessageId = assurBoxNotification.MessageId,
                Communication = acceptation,
                IsTerminationRequestAccepted = acceptation == "OK",
                EffectiveTerminationDate = DateTime.Today.AddDays(7)

            }).Result;

            // todo change "green card"
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                var request = ctx.CarGreenCardRequests.FirstOrDefault(x => x.RequestId == key);
                request.ResponseInfo = "Response sent  " + resp.ResponseContent;
                request.RequestRespondDate = DateTime.UtcNow;
                ctx.SaveChanges();
            }
        }

        private string GetAcceptation(SDK.DTO.Risk risk)
        {
            switch (risk.RiskType)
            {
                case SDK.DTO.RiskTypes.Vehicle:
                    {
                        switch (risk.Identifier.ToUpper())
                        {
                            case "ER0404":
                                {
                                    return "Validation Error : we can't terminate this contract.";
                                }
                            case "ER0403":
                                {
                                    return "Validation Error : we can't terminate this contract  -- reason xyz.";
                                }
                            default: { } break;
                        }
                    }
                    break;
                default: { } break;

            }
            return "OK";
        }
    }
}
