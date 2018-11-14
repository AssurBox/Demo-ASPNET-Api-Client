using AssurBox.Samples.Client.Garage.Web.Core;
using AssurBox.SDK;
using AssurBox.SDK.DTO.GreenCard.Car;
//using Bogus;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AssurBox.Samples.Client.Garage.Web.Controllers
{
    public class GreenCardRequestsController : Controller
    {
        // GET: GreenCardRequests
        public async Task<ActionResult> Index(int page = 0, SDK.DTO.GreenCard.GreenCardRequestStatus status = SDK.DTO.GreenCard.GreenCardRequestStatus.All)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }
            if (page < 0)
            {
                page = 0;
            }

            Stopwatch watch = Stopwatch.StartNew();
            // retrieves the list of requests : note that this is for demo purpose, 
            // in real life application you should keep the correlationid of the greencard request and retrieve the response when the user needs it
            using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(new AssurBoxClientOptions { Environment = AssurBoxEnvironments.Test, ApiKey = SessionManager.BearerToken }))
            {
                try
                {
                    var requests = await client.GetRequests(page, 50, new System.Threading.CancellationToken(), status);
                    watch.Stop();
                    ViewBag.Time = watch.ElapsedMilliseconds;
                    return View(requests);
                }
                catch (AssurBoxException aex)
                {
                    ViewBag.Errors = GetError(aex);
                }
            }
            return View();
        }

        private string GetError(AssurBoxException aex)
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("<p>Error {0} : {1}</p>", aex.ErrorCode, aex.Message);
            try
            {
                if (aex.ErrorDetail != null && aex.ErrorDetail.Error != null)
                {
                    b.AppendFormat("<p>Error {0} : {1}</p>", aex.ErrorDetail?.Error?.Code, aex.ErrorDetail?.Error?.Message);

                    foreach (var err in aex.ErrorDetail.Error.Details)
                    {
                        b.AppendFormat("<p>Error {0} : {1}</p>", err.Code, err.Message);
                    }
                }
                else
                {
                    b.AppendFormat("<p>Error {0} : {1}</p>", aex?.ErrorDetail?.Error?.Code, aex?.ErrorDetail?.Error?.Message);
                }
            }
            catch
            {
            }
            return b.ToString();
        }

        // GET: GreenCardRequests/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }
            using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(new AssurBoxClientOptions { Environment = AssurBoxEnvironments.Test, ApiKey = SessionManager.BearerToken }))
            {
                var request = await client.GetRequest(id);
                SaveFilesInTempDir(request);

                return View(request);
            }
        }

        public async Task<FileResult> DownloadSNCADoc(Guid id)
        {
            using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(new AssurBoxClientOptions { Environment = AssurBoxEnvironments.Test, ApiKey = SessionManager.BearerToken }))
            {
                var snca = await client.GetDocumentSNCA(id);
                return File(Convert.FromBase64String(snca.Content), snca.Type, snca.Filename);
            }
        }

        /// <summary>
        /// We use a temp dir for demo purpose
        /// </summary>
        /// <param name="request"></param>
        private static void SaveFilesInTempDir(SDK.DTO.GreenCard.Car.GreenCardRequestInfo request)
        {
            string tempdir = System.Web.Hosting.HostingEnvironment.MapPath("~/tempdir/");
            Directory.CreateDirectory(tempdir);
            if (request.Attachments != null && request.Attachments.Any())
            {
                foreach (var attachement in request.Attachments)
                {
                    string filename = GetFileName(request, attachement);
                    var filepath = Path.Combine(tempdir, filename);
                    System.IO.File.WriteAllBytes(filepath, Convert.FromBase64String(attachement.Content));
                }
            }
        }

        // for demo purpose: files are saved with a convention      
        private static string GetFileName(SDK.DTO.GreenCard.Car.GreenCardRequestInfo request, SDK.Attachment attachement)
        {
            return $"{request.CorrelationId}{attachement.Filename}";
        }

        // GET: GreenCardRequests/Create
        public async Task<ActionResult> Create()
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(new AssurBoxClientOptions { Environment = AssurBoxEnvironments.Test, ApiKey = SessionManager.BearerToken }))
                {
                    ViewBag.Insurances = await client.GetInsurancesGreenCardsIssuers();
                }
            }
            catch (AssurBoxException aex)
            {
                ModelState.AddModelError("", aex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }


            var model = FakeData.GetGreenCardRequest();

            return View(model);
        }

        // POST: GreenCardRequests/Create
        [HttpPost]
        public async Task<ActionResult> Create(GreenCardRequestInitialization model)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(new AssurBoxClientOptions { Environment = AssurBoxEnvironments.Test, ApiKey = SessionManager.BearerToken }))
                {
                    try
                    {
                        var creationResult = await client.NewRequest(model);
                        return RedirectToAction("Details", new { id = creationResult.ResponseContent });
                    }
                    catch (AssurBoxException ex)
                    {
                        ModelState.AddModelError("", "An error occured " + ex.Message + " code " + ex.ErrorCode);
                        try
                        {
                            if (ex.ErrorDetail != null)
                            {
                                ModelState.AddModelError("", ex?.ErrorDetail?.Error?.Message);
                                foreach (var err in ex.ErrorDetail.Error.Details)
                                {
                                    ModelState.AddModelError("", err.Message);
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", ex?.ErrorDetail?.Error?.Message);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(new AssurBoxClientOptions { Environment = AssurBoxEnvironments.Test, ApiKey = SessionManager.BearerToken }))
            {
                ViewBag.Insurances = await client.GetInsurancesGreenCardsIssuers();
            }
            return View(model);
        }

        // GET: GreenCardRequests/Edit/5
        public async Task<ActionResult> Edit(Guid id)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }

            GreenCardRequestModification model = new GreenCardRequestModification { CorrelationId = id };
            return View(model);
        }

        // POST: GreenCardRequests/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(GreenCardRequestModification model)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(new AssurBoxClientOptions { Environment = AssurBoxEnvironments.Test, ApiKey = SessionManager.BearerToken }))
                {
                    try
                    {
                        var creationResult = await client.ChangeRequest(model);
                        return RedirectToAction("Details", new { id = creationResult.ResponseContent });
                    }
                    catch (AssurBoxException ex)
                    {
                        ModelState.AddModelError("", "An error occured " + ex.Message);
                        try
                        {
                            ModelState.AddModelError("", ex.ErrorDetail.Error.Message);
                            foreach (var err in ex.ErrorDetail.Error.Details)
                            {
                                ModelState.AddModelError("", err.Message);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return View(model);
        }

        // GET: GreenCardRequests/Delete/5
        public async Task<ActionResult> Delete(Guid id)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }
            GreenCardRequestCancellation model = new GreenCardRequestCancellation { CorrelationId = id };
            return View(model);
        }

        // POST: GreenCardRequests/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(GreenCardRequestCancellation model)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(new AssurBoxClientOptions { Environment = AssurBoxEnvironments.Test, ApiKey = SessionManager.BearerToken }))
                {
                    try
                    {
                        var creationResult = await client.CancelRequest(model);
                        return RedirectToAction("Index");
                    }
                    catch (AssurBoxException ex)
                    {
                        ModelState.AddModelError("", "An error occured " + ex.Message);
                        try
                        {
                            ModelState.AddModelError("", ex.ErrorDetail.Error.Message);
                            foreach (var err in ex.ErrorDetail.Error.Details)
                            {
                                ModelState.AddModelError("", err.Message);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return View(model);
        }
    }
}
