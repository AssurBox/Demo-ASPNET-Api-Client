using AssurBox.SDK;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AssurBox.Samples.API.Insurance.Controllers
{
    // ?risk=&identifiertype=&identifier=&cover=&cover=
    /// <summary>
    /// This is the endpoint to configure in AssurBox by the insurance
    /// </summary>
    public class CoverageCheckController : ApiController
    {
        public HttpResponseMessage Get(int risk, int identifierType, string identifier, int[] cover)
        {
            //todo : simulate errors for specific parameters
            switch (risk)
            {
                case 1:
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "NEW POLICY {information}");
                    }
                default:
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ApiError("400", "This service is not implemented by Insurance Delta"));
                    }
            }

        }


        // simulates business logic

        private string GetVehicleCoverage(string identifier)
        {
            // cover

            // errors ?
            switch (identifier.ToUpper())
            {
                case "ER0404":
                    {
                        return "This vehicule is not insured by [assurance name - config]";
                    }
                default: { }break;
            }

            return "";

        }


    }
}
