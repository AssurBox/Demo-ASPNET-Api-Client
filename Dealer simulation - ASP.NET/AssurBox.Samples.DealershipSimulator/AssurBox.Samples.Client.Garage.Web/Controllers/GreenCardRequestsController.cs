using AssurBox.Samples.Client.Garage.Web.Core;
using AssurBox.SDK;
using AssurBox.SDK.DTO.GreenCard.Car;
using Bogus;
//using Bogus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AssurBox.Samples.Client.Garage.Web.Controllers
{
    public class GreenCardRequestsController : Controller
    {
        // GET: GreenCardRequests
        public async Task<ActionResult> Index()
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }
            using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(SessionManager.BearerToken))
            {
                var requests = await client.GetRequests();
                return View(requests);
            }
        }

        // GET: GreenCardRequests/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.BearerToken))
            {
                return RedirectToAction("Index", "Home");
            }
            using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(SessionManager.BearerToken))
            {
                var request = await client.GetRequest(id);
                SaveFilesInTempDir(request);

                return View(request);
            }
        }

        public async Task<FileResult> DownloadSNCADoc(Guid id)
        {
            using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(SessionManager.BearerToken))
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

            using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(SessionManager.BearerToken))
            {
                ViewBag.Insurances = await client.GetInsurancesGreenCardsIssuers();
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
                using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(SessionManager.BearerToken))
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
            using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(SessionManager.BearerToken))
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
                using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(SessionManager.BearerToken))
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
                using (SDK.Clients.CarGreenCardClient client = new SDK.Clients.CarGreenCardClient(SessionManager.BearerToken))
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
