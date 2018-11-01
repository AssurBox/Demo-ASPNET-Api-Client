
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;

namespace AssurBox.Samples.API.Insurance.Controllers
{
    public class EchoController : ApiController
    {
        public HttpResponseMessage Get(string id)
        {
            
            return Request.CreateResponse(HttpStatusCode.OK, id);
        }
    }
}
