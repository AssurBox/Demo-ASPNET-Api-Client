
using AssurBox.SDK;
using AssurBox.SDK.Clients;
using AssurBox.SDK.DTO.GreenCard.Car;
using AssurBox.SDK.WebHooks;
using Hangfire;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;

namespace AssurBox.Samples.API.Insurance.Controllers
{
    /// <summary>
    /// This controller shows a wah to define a webhook handler for AssurBox
    /// </summary>
    public class CarGreenCardController : ApiController
    {
        //http://docs.hangfire.io/en/latest/background-methods/writing-unit-tests.html
        private readonly IBackgroundJobClient _jobClient;
        public CarGreenCardController() : this(new BackgroundJobClient())
        {
        }

        public CarGreenCardController(IBackgroundJobClient jobClient)
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
        [Route("webhook/GreenCard")]
        [HttpPost]
        public HttpResponseMessage Post(SDK.WebHooks.GreenCardRequestNotification value)
        {
            if (value == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Parameter can't be null");
            }

            Logger.Log($"CarGreenCardController.Post - Debut {value.CorrelationId}");

            string key = $"{value.CorrelationId}|{value.MessageId}";

            // Save the notification for later processing
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                ctx.CarGreenCardRequests.Add(new DAL.CarGreenCardRequest
                {
                    RequestDate = DateTime.UtcNow,
                    RequestId = key,
                    RawRequest = JsonConvert.SerializeObject(value, Formatting.None),
                });
                ctx.SaveChanges();
            }

            // Creates a job that will be executed as soon as possible
            _jobClient.Enqueue(() => ExecuteJob(key));

            Logger.Log("CarGreenCardController.Post - Fin");

            // Don't forget to notify AssurBox that the notication is received
            return Request.CreateResponse(HttpStatusCode.OK, value.CorrelationId);
        }

        /// <summary>
        /// Ici on va traiter la demande que l'on a sauvegardée 
        /// on récupère les infos
        /// on crée une carte verte ou tout ce qu'on veut
        /// et on envoie la réponse à AssurBox via l'api
        /// </summary>
        /// <param name="key"></param>
        public void ExecuteJob(string key)
        {
            Logger.Log($"CarGreenCardController.ExecuteJob - Debut {key}");

            // recherche dans la table 
            GreenCardRequestNotification assurBoxNotification;
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                var request = ctx.CarGreenCardRequests.AsNoTracking().FirstOrDefault(x => x.RequestId == key);
                assurBoxNotification = JsonConvert.DeserializeObject<SDK.WebHooks.GreenCardRequestNotification>(request.RawRequest);
            }

            switch (assurBoxNotification.NotificationType)
            {
                case SDK.WebHooks.GreenCardRequestNotificationTypes.InitialRequest:
                    {
                        // generates a sample green card and send it back to AssurBox
                        ProcessInitialRequest(key, assurBoxNotification);

                    }
                    break;
                case SDK.WebHooks.GreenCardRequestNotificationTypes.Modification:
                    {
                        // generates a sample green card and send it back to AssurBox
                        ProcessModificationRequest(key, assurBoxNotification);
                    }
                    break;
                case SDK.WebHooks.GreenCardRequestNotificationTypes.Cancelation:
                    {
                        ProcessCancellationRequest(key, assurBoxNotification);

                    }
                    break;
                default: { } break;
            }





        }



        /// <summary>
        /// Handle a notification for a green card request
        /// </summary>
        /// <param name="key"></param>
        /// <param name="assurBoxNotification"></param>
        private void ProcessInitialRequest(string key, GreenCardRequestNotification assurBoxNotification)
        {
            // 1. retrieve a token to access the AssurBox Api
            var tokenInfo = GetAssurBoxSecurityToken();

            // 2. using the token, create a "greencard client"
            CarGreenCardClient client = new CarGreenCardClient(new AssurBoxClientOptions { Host = Config.AssurBoxApiBaseURL, ApiKey = tokenInfo });

            // 3. using the authentified client, retrieve the details of the green card request 
            //    full detail about the customer, car information, ...
            var requestDetails = client.GetRequest(assurBoxNotification.CorrelationId).Result;

            // 4. do something with the request in your information system
            var document = GetDemoPDF(requestDetails);

            // 5. send a response to the requester, using the greencard client and a greencardrequestresponse object
            GreenCardRequestResponse response = new GreenCardRequestResponse();
            // The correlation id identify a greencard request file (this is mandatory)
            response.CorrelationId = assurBoxNotification.CorrelationId;
            // The messageid identify a specific message (this is mandatory)
            response.MessageId = assurBoxNotification.MessageId;

            string validationMessage = "";
            if (ValidateRequest(assurBoxNotification, out validationMessage) == false)
            {
                // We can send a response refusing to issue a green card
                response.HasApproval = false;
                response.ApprovalReason = validationMessage;
                response.ResponseContent = validationMessage;
            }
            else
            {
                // or we can send the green card  back
                response.HasApproval = true; // don't forget to set this property to true

                // define a message for the requester
                response.ResponseContent = $@"
                    Bonjour {assurBoxNotification.Requester.Name},

                    Merci pour votre demande, (type : {assurBoxNotification.NotificationType})

                    Voici votre carte pour {assurBoxNotification.LicencePlate}

                    Client : {requestDetails.CustomerName}

                    Bien à vous,
                    Assurance simulation demo ({requestDetails.InsuranceName})
                    ";

                // make sure the file is encoded as a base64 string
                response.AddAttachment($"CarteVerte_{assurBoxNotification.LicencePlate}.pdf", Convert.ToBase64String(document));
            }

            // send the response to AssurBox
            var resp = client.SendResponse(response).Result;

            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                var request = ctx.CarGreenCardRequests.FirstOrDefault(x => x.RequestId == key);
                request.ResponseInfo = "Response sent  " + resp.ResponseContent;
                request.RequestRespondDate = DateTime.UtcNow;
                ctx.SaveChanges();
            }
        }

        private bool ValidateRequest(GreenCardRequestNotification assurBoxNotification, out string validationMessage)
        {
            validationMessage = string.Empty;
            // in order to test, we defined that some Licence Plates should simulate a bad request
            switch (assurBoxNotification.LicencePlate.ToUpper())
            {
                case "ER0404":
                    {
                        validationMessage = "Validation Error : we can't issue a green card for this client.";
                    }
                    break;
                case "ER0403":
                    {
                        validationMessage = "Validation Error : we can't issue a green card for this vehicle.";
                    }
                    break;
                default: break;
            }


            return string.IsNullOrEmpty(validationMessage);
        }

        /// <summary>
        /// Handle a notification for a green card request
        /// </summary>
        /// <param name="key"></param>
        /// <param name="assurBoxNotification"></param>
        private void ProcessModificationRequest(string key, GreenCardRequestNotification assurBoxNotification)
        {
            // 1. retrieve a token to access the AssurBox Api
            var tokenInfo = GetAssurBoxSecurityToken();

            // 2. using the token, create a "greencard client"
            CarGreenCardClient client = new CarGreenCardClient(new AssurBoxClientOptions { Host = Config.AssurBoxApiBaseURL, ApiKey = tokenInfo });

            // 3. using the authentified client, retrieve the details of the green card request 
            //    full detail about the customer, car information, ...
            var requestDetails = client.GetRequest(assurBoxNotification.CorrelationId).Result;

            // 4. do something with the request in your information system
            var document = GetDemoPDFUpdate(requestDetails);

            // 5. send a response to the requester, using the greencard client and a greencardrequestresponse object
            GreenCardRequestResponse response = new GreenCardRequestResponse();
            // The correlation id identify a greencard request file (this is mandatory)
            response.CorrelationId = assurBoxNotification.CorrelationId;
            // The messageid identify a specific message (this is mandatory)
            response.MessageId = assurBoxNotification.MessageId;

            response.HasApproval = true; // don't forget to set this property to true

            // define a message for the requester
            response.ResponseContent = $@"
                    Bonjour {assurBoxNotification.Requester.Name},

                    Merci pour votre demande, (type : {assurBoxNotification.NotificationType})

                    Voici votre carte mise à jour pour {assurBoxNotification.LicencePlate}

                    Client : {requestDetails.CustomerName}

                    Bien à vous,
                    Assurance simulation demo ({requestDetails.InsuranceName})
                    ";

            // make sure the file is encoded as a base64 string
            response.AddAttachment($"CarteVerte_{assurBoxNotification.LicencePlate}.pdf", Convert.ToBase64String(document));


            // send the response to AssurBox
            var resp = client.SendResponse(response).Result;

            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                var request = ctx.CarGreenCardRequests.FirstOrDefault(x => x.RequestId == key);
                request.ResponseInfo = "Updated response  " + resp.ResponseContent;
                request.RequestRespondDate = DateTime.UtcNow;
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Handle a notification that a Green card request is canceled
        /// </summary>
        /// <param name="key"></param>
        /// <param name="assurBoxNotification"></param>
        private void ProcessCancellationRequest(string key, GreenCardRequestNotification assurBoxNotification)
        {
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                var request = ctx.CarGreenCardRequests.FirstOrDefault(x => x.RequestId == key);
                request.ResponseInfo = "The green card request was cancelled : " + assurBoxNotification.Communication;
                request.RequestRespondDate = DateTime.UtcNow;
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the JWT token to communicate with AssurBox
        /// </summary>
        /// <returns></returns>
        private string GetAssurBoxSecurityToken()
        {

            string token = MemoryCache.Default["ABX_TOKEN"] as string;
            if (string.IsNullOrEmpty(token))
            {
                SecurityClient id = new SecurityClient(new AssurBoxClientOptions { Host = Config.AssurBoxApiBaseURL });
                var tokeninfo = id.GetBearerToken(Config.AssurBoxApiClientID, Config.AssurBoxApiClientSecret).Result;
                token = tokeninfo.access_token;
                MemoryCache.Default["ABX_TOKEN"] = token;
            }
            return token;

        }



        #region *** Remplissage simulation carte verte ***

        private struct CarteVerteSimulationFields
        {
            public const string LicencePlate = "LicencePlate";
            public const string VIN = "VIN";
            public const string infos = "Other";
        }


        /// <summary>
        /// Simulates a green card
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public byte[] GetDemoPDF(SDK.DTO.GreenCard.Car.GreenCardRequestInfo request)
        {
            string pdfSourcePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/cartevertetemplate.pdf");

            using (var pdfReader = new PdfReader(pdfSourcePath))
            {
                var memoryStream = new MemoryStream();
                var pdfStamper = new PdfStamper(pdfReader, memoryStream);
                var pdfFormFields = pdfStamper.AcroFields;


                pdfFormFields.SetField(CarteVerteSimulationFields.LicencePlate, request.LicencePlate);
                pdfFormFields.SetField(CarteVerteSimulationFields.VIN, request.VIN);

                string infos = "";
                if (request.RequestDetails.VehicleOwner.IsCompany)
                {
                    infos = $@"CREATION  Client COMPANY : {request.RequestDetails.VehicleOwner.Company.Name} 

Vehicule : {request.RequestDetails.CarDetails.Make} {request.RequestDetails.CarDetails.Model}

Généré le {DateTime.UtcNow} (UTC)
";
                }
                else
                {
                    infos = $@"CREATION  Client : {request.RequestDetails.VehicleOwner.Person.FirstName} {request.RequestDetails.VehicleOwner.Person.LastName} 

Vehicule : {request.RequestDetails.CarDetails.Make} {request.RequestDetails.CarDetails.Model}

Généré le {DateTime.UtcNow} (UTC)
";
                }
                pdfFormFields.SetField(CarteVerteSimulationFields.infos, infos);//infos

                pdfStamper.FormFlattening = false;
                pdfStamper.Close();

                return memoryStream.ToArray();
            }

        }

        public byte[] GetDemoPDFUpdate(SDK.DTO.GreenCard.Car.GreenCardRequestInfo request)
        {



            string pdfSourcePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/cartevertetemplate.pdf");

            using (var pdfReader = new PdfReader(pdfSourcePath))
            {
                var memoryStream = new MemoryStream();
                var pdfStamper = new PdfStamper(pdfReader, memoryStream);
                var pdfFormFields = pdfStamper.AcroFields;


                pdfFormFields.SetField(CarteVerteSimulationFields.LicencePlate, request.LicencePlate);
                pdfFormFields.SetField(CarteVerteSimulationFields.VIN, request.VIN);

                string infos = $@"
UPDATE

Généré le {DateTime.UtcNow} (UTC)
";
                pdfFormFields.SetField(CarteVerteSimulationFields.infos, infos);

                pdfStamper.FormFlattening = false;
                pdfStamper.Close();

                return memoryStream.ToArray();
            }

        }
        #endregion *** Remplissage simulation carte verte ***
    }
}
