using System.Web.Mvc;
using CmsWeb.Areas.Search.Models;
using UtilityExtensions;

namespace CmsWeb.Areas.Search.Controllers
{
    [RouteArea("Search", AreaPrefix = "TaskSearch"), Route("{action}/{id?}")]
    public class TaskSearchController : CmsStaffController
    {
        [HttpGet, Route("~/TaskSearch")]
        public ActionResult Index()
        {
            Response.NoCache();
            var m = new TaskSearchModel();

            m.Search.GetPreference();
            return View(m);
        }

        [HttpPost]
        public ActionResult Results(TaskSearchModel m)
        {
            m.Search.SavePreference();
            return View(m);
        }

        [HttpPost]
        public ActionResult Clear()
        {
            var m = new TaskSearchModel();
            m.Search.ClearPreference();
            return Redirect("/TaskSearch");
        }
        public ActionResult DelegateNames(string term, string options)
        {
            var n = TaskSearchModel.FindNames("Delegate", term, 10, options);
            return Json(n, JsonRequestBehavior.AllowGet);
        }
        public ActionResult OwnerNames(string term, string options)
        {
            var n = TaskSearchModel.FindNames("Owner", term, 10, options);
            return Json(n, JsonRequestBehavior.AllowGet);
        }
        public ActionResult OriginatorNames(string term, string options)
        {
            var n = TaskSearchModel.FindNames("Originator", term, 10, options);
            return Json(n, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AboutNames(string term, string options)
        {
            var n = TaskSearchModel.FindNames("About", term, 10, options);
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Archive(TaskSearchModel m)
        {
            m.Archive();
            m.Search.SavePreference();
            return Redirect("/TaskSearch");
        }
        [HttpPost]
        public ActionResult UnArchive(TaskSearchModel m)
        {
            m.Archive();
            m.Search.SavePreference();
            return Redirect("/TaskSearch");
        }
        [HttpPost]
        public ActionResult Delete(TaskSearchModel m)
        {
            m.Delete();
            m.Search.SavePreference();
            return Redirect("/TaskSearch");
        }
        [HttpPost]
        public ActionResult Delegate(int id, TaskSearchModel m)
        {
            m.Delegate(id);
            m.Search.SavePreference();
            return Redirect("/TaskSearch");
        }
    }
}
