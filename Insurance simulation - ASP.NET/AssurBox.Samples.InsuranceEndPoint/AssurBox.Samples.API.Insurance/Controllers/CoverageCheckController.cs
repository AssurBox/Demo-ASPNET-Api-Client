using System.Web.Http;

namespace AssurBox.Samples.API.Insurance.Controllers
{
    // ?risk=&identifiertype=&identifier=&cover=&cover=
    public class CoverageCheckController : ApiController
    {
        public string Get(int risk, int identifierType, string identifier, int[] cover)
        {
            //todo : simulate errors for specific parameters
            return "";
        }


    }
}
