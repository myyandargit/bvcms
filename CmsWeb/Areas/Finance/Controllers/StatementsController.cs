using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;
using CmsData;
using CmsData.API;
using CmsWeb.Areas.Finance.Models.Report;
using CmsWeb.Models;
using UtilityExtensions;

namespace CmsWeb.Areas.Finance.Controllers
{
    [Authorize(Roles = "Finance")]
    [RouteArea("Finance", AreaPrefix = "Statements"), Route("{action}")]
    public class StatementsController : CmsController
    {
        [Route("~/Statements")]
        public ActionResult Index()
        {
            var r = DbUtil.Db.ContributionsRuns.OrderByDescending(mm => mm.Id).FirstOrDefault();
            if (r != null && r.Running == 1 && DateTime.Now.Subtract(r.Started ?? DateTime.MinValue).TotalMinutes < 1)
                return Redirect("/Statements/Progress");
            ViewBag.Previous = r != null;
            return View();
        }

        [HttpPost, Route("Start")]
        public ActionResult ContributionStatements(DateTime? fromDate, DateTime? endDate, string startswith, string sort, int? tagid, bool excludeelectronic, bool exportcontributors = false)
        {
            if (!fromDate.HasValue || !endDate.HasValue)
                return Content("<h3>Must have a Startdate and Enddate</h3>");
            var runningtotals = new ContributionsRun
            {
                Started = DateTime.Now,
                Count = 0,
                Processed = 0
            };
            if (!startswith.HasValue())
                startswith = null;
            if (exportcontributors)
            {
                var db = DbUtil.Db;
                var noaddressok = !db.Setting("RequireAddressOnStatement", true);
                const bool useMinAmt = true;
                if (tagid == 0)
                    tagid = null;
                var qc = APIContribution.contributors(db, fromDate.Value, endDate.Value, 0, 0, 0, noaddressok, useMinAmt, startswith, sort, tagid: tagid, excludeelectronic: excludeelectronic);
                return ExcelExportModel.ToDataTable(qc.ToList()).ToExcel("Contributors.xlsx");
            }
            DbUtil.Db.ContributionsRuns.InsertOnSubmit(runningtotals);
            DbUtil.Db.SubmitChanges();
            var cul = DbUtil.Db.Setting("Culture", "en-US");
            var host = Util.Host;

            var output = Output();
            if (tagid == 0)
                tagid = null;

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cul);
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cul);
                var m = new ContributionStatementsExtract(host, fromDate.Value, endDate.Value, output, startswith, sort, tagid, excludeelectronic);
                m.DoWork();
            });
            return Redirect("/Statements/Progress");
        }

        public ActionResult SomeTaskCompleted(string result)
        {
            return Content(result);
        }

        private static string Output()
        {
            var output = ConfigurationManager.AppSettings["SharedFolder"].Replace("%USERPROFILE%", Environment.GetEnvironmentVariable("USERPROFILE"));
            output = output + $"/Statements/contributions_{Util.Host}.pdf";
            return output;
        }


        [HttpGet]
        public ActionResult Progress()
        {
            var r = DbUtil.Db.ContributionsRuns.OrderByDescending(mm => mm.Id).First();
            var html = new StringBuilder();
            if(r.CurrSet > 0)
                html.Append("<a href=\"/Statements/Download\">PDF with all households</a><br>");
            if (r.Sets.HasValue())
            {
                var sets = r.Sets.Split(',').Select(ss => ss.ToInt()).ToList();
                foreach (var set in sets)
                    html.Append($"<a href=\"/Statements/Download/{set}\">PDF with {set} pages per household</a><br>");
            }
            ViewBag.download = html.ToString();
            return View(r);
        }

        [HttpGet, Route("~/Statements/Download/{id:int?}")]
        public ActionResult Download(int? id)
        {
            var output = Output();
            var fn = output;
            if (id.HasValue)
                fn = ContributionStatementsExtract.Output(output, id.Value);
            if (!System.IO.File.Exists(fn))
                return Content("no pending download");
            return new ContributionStatementsResult(fn);
        }
    }
}
