
using AssurBox.SDK.EndpointContracts.Insurers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AssurBox.Samples.API.Insurance.Controllers
{
    /// <summary>
    /// Mise a disposition d'une recherche parmis la base client assureur
    /// (Faciliter la saisie via l'application web)
    /// </summary>
    public class ClientsController : ApiController
    {
        public HttpResponseMessage Get(string searchpattern)
        {
            List<CustomerInfo> clientsDemo = new List<CustomerInfo>();
            clientsDemo.Add(new CustomerInfo
            {
                Reference = "referencexxx",
                FirstName = "PrénomXXX",
                LastName = "NomXXX",
                Address = "Rue de YYY",
                BirthDate = new DateTime(1984, 12, 10)
            });
            clientsDemo.Add(new CustomerInfo
            {
                Reference = "referencezzz",
                FirstName = searchpattern,
                LastName = "NomZZZ",
                Address = "Rue de ZZZ",
                BirthDate = new DateTime(1987, 07, 05)
            });
            return Request.CreateResponse(HttpStatusCode.OK, clientsDemo);
        }
    }
}
