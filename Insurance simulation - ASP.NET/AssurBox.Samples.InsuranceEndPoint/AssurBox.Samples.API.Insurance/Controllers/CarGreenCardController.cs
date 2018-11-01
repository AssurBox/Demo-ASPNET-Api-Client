
using AssurBox.SDK;
using AssurBox.SDK.Clients;
using AssurBox.SDK.DTO.GreenCard.Car;
using Hangfire;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AssurBox.Samples.API.Insurance.Controllers
{
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

        public class InsuranceApiCarGreenCardMessage : ApiMessage//<InsuranceApiCarGreenCardMessage>
        {
            public string Licence { get; set; }
        }
        [HttpPost]
        public HttpResponseMessage Post(SDK.EndpointContracts.Insurers.GreenCardRequestContract value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }


            Logger.Log($"CarGreenCardController.Post - Debut {value.CorrelationId}");

            // ici on va récupérer la demande qui vient d'AssurBox, la sauver et confirmer qu'on l'a bien reçue.
            // Si la réception du message n'est pas confirmée, AssurBox va tenter de ré-envoyer le message pendant un temps
            // et si la réception n'est toujours pas confirmée, il enverra un email "fallback" avec la demande.

            string key = $"{value.CorrelationId}|{value.MessageId}";

            // sauver dans la table requetes
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                ctx.CarGreenCardRequests.Add(new DAL.CarGreenCardRequest
                {
                    RequestDate = DateTime.UtcNow,
                    RequestId = key,
                    RawRequest = JsonConvert.SerializeObject(value, Formatting.Indented),
                });
                ctx.SaveChanges();
            }

            // Créer 1 job hangfire qui répondra a la requete des que possible
            _jobClient.Enqueue(() => ExecuteJob(key));

            Logger.Log("CarGreenCardController.Post - Fin");
            // ok bien reçu requete xxx

            return Request.CreateResponse(HttpStatusCode.OK, value.CorrelationId);
        }

        public void ExecuteJob(string key)
        {

            Logger.Log($"CarGreenCardController.ExecuteJob - Debut {key}");

            //InsuranceApiCarGreenCardMessage 

            /*
             *  ^
             * /!\
             * ---
             * 
             * 
             Todo : 

            dessiner toutes les relations pour éviter de dupliquer des pocos...

            Il faut une vue globale et claire sinon on s'en sortira pas
             
             */

            // Ici on va traiter la demande que l'on a sauvegardée 
            // on récupère les infos
            // on crée une carte verte ou tout ce qu'on veut
            // et on envoie la réponse à AssurBox

            // ici on va utiliser l'api AssurBox pour envoyer la réponse (carte verte)

            // recherche dans la table 
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                var request = ctx.CarGreenCardRequests.FirstOrDefault(x => x.RequestId == key);
                var requestInformation = JsonConvert.DeserializeObject<SDK.EndpointContracts.Insurers.GreenCardRequestContract>(request.RawRequest);
                //generer carte verte et l'envoyer

                Logger.Log($"CarGreenCardController.ExecuteJob - request get {request.RequestId}", request.RawRequest);

                //  string cvepath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Simulation Carte Verte.pdf");
                if (requestInformation.CommunicationType == SDK.EndpointContracts.Insurers.CommunicationType.Cancelation)
                {
                    request.ResponseInfo = "Cancelled green card request (simulation)";
                }
                else
                {
                    IdentityClient id = new IdentityClient("", Config.AssurBoxApiBaseURL);

                    var tokenInfo = id.GetBearerToken(Config.AssurBoxApiClientID, Config.AssurBoxApiClientSecret).Result;

                    CarGreenCardClient client = new CarGreenCardClient(tokenInfo.access_token, Config.AssurBoxApiBaseURL);

                    var requestDetails = client.GetRequest(requestInformation.CorrelationId).Result;


                    GreenCardRequestResponse response = new GreenCardRequestResponse();
                    response.CorrelationId = requestInformation.CorrelationId;

                    response.MessageId = requestInformation.MessageId;

                    if (string.IsNullOrEmpty(requestInformation.Communication) == false && requestInformation.Communication.StartsWith("THROWERROR", StringComparison.InvariantCultureIgnoreCase))
                    {
                        response.HasApproval = false;
                        response.ApprovalReason = "Client interdit d'assurance.";
                        response.ResponseContent = "Error : client not valid";
                    }
                    else
                    {
                        response.HasApproval = true;
                        response.ResponseContent = $@"
Bonjour {requestInformation.Requester.Name},

Merci pour votre demande, (type : {requestInformation.CommunicationType})

Voici votre carte pour {requestInformation.LicencePlate}

Client : {requestDetails.CustomerName}


Bien à vous,
Assurance simulation demo ({requestDetails.InsuranceName})
";
                        string base64 = requestInformation.CommunicationType == SDK.EndpointContracts.Insurers.CommunicationType.InitialRequest ? Convert.ToBase64String(GetDemoPDF(requestDetails)) : Convert.ToBase64String(GetDemoPDFUpdate(requestDetails));

                        response.AddAttachment($"CarteVerte{request.Id}.pdf", base64);
                    }

                    Logger.Log($"CarGreenCardController.ExecuteJob - request fichier attaché");




                    var resp = client.SendResponse(response).Result;

                    if (requestInformation.CommunicationType == SDK.EndpointContracts.Insurers.CommunicationType.InitialRequest)
                    {
                        request.ResponseInfo = "Renvoyé  " + resp.ResponseContent;
                    }
                    else
                    {
                        request.ResponseInfo = "Renvoyé mise à jour " + resp.ResponseContent;
                    }
                }
                request.RequestRespondDate = DateTime.UtcNow;
                ctx.SaveChanges();

                Logger.Log($"CarGreenCardController.ExecuteJob - etat sauvé");
            }
        }



        #region *** Remplissage simulation carte verte ***

        private struct CarteVerteSimulationFields
        {
            public const string LicencePlate = "LicencePlate";
            public const string VIN = "VIN";
            public const string infos = "Other";
        }



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
                if (request.RequestDetails.Customer.IsCompany)
                {
                    infos = $@"CREATION  Preneur COMPANY : {request.RequestDetails.Customer.Company.Name} 

Vehicule : {request.RequestDetails.CarDetails.Make} {request.RequestDetails.CarDetails.Model}

Généré le {DateTime.UtcNow} (UTC)
";
                }
                else
                {
                    infos = $@"CREATION  Preneur : {request.RequestDetails.Customer.Person.FirstName} {request.RequestDetails.Customer.Person.LastName} 

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
