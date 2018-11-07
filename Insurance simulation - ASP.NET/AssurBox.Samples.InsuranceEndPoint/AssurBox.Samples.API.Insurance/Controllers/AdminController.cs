using AssurBox.Samples.API.Insurance.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AssurBox.Samples.API.Insurance.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public async Task<ActionResult> Index()
        {
            AssurBox.Samples.API.Insurance.Models.AdminModel model = new Models.AdminModel();
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                model.Requests = await ctx.CarGreenCardRequests.AsNoTracking()
                    .OrderByDescending(x => x.RequestDate).ToListAsync();
            }
            return View(model);
        }

        public async Task<ActionResult> RequestDetail(int requestID)
        {

            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                var requ = await ctx.CarGreenCardRequests.FindAsync(requestID);
                return View(requ);
            }

        }

        public async Task<ActionResult> Logs()
        {
            LogsModel model = new LogsModel();
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                model.Logs = await ctx.Logs.OrderByDescending(x => x.LogDate).ToListAsync();
            }
            return View(model);
        }
    }
}