using AssurBox.Samples.API.Insurance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AssurBox.Samples.API.Insurance.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            AssurBox.Samples.API.Insurance.Models.AdminModel model = new Models.AdminModel();
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                model.Requests = ctx.CarGreenCardRequests.OrderByDescending(x => x.RequestDate).ToList();
            }
            return View(model);
        }

        public ActionResult RequestDetail(int requestID)
        {
            
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                var requ = ctx.CarGreenCardRequests.Find(requestID);
                return View(requ);
            }
            
        }

        public ActionResult Logs()
        {
            LogsModel model = new LogsModel();
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                model.Logs = ctx.Logs.OrderByDescending(x => x.LogDate).ToList();
            }
            return View(model);
        }
    }
}