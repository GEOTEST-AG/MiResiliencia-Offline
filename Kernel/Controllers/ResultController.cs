using NHibernate;
using ResTB_API.Helpers;
using ResTB_API.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ResTB_API.Controllers
{
    public static class ResultWrapper
    {

        public static void CreateDamageExtents(int projectId)
        {
            var potController = new MappedObjectController();
            potController.createDamageExtent(projectId);

        }

        //public static string RunCalculationGetSummary(int projectId)
        //{
        //    var potController = new MappedObjectController();
        //    potController.createDamageExtent(projectId);

        //    return GetSummary(projectId);
        //}

        public static ProjectResult ComputeResult(int projectId, bool details = false)
        {
            var _controller = new DamageExtentController();
            ProjectResult _result = _controller.computeProjectResult(projectId, details);

            return _result;
        }

    }

    /// <summary>
    /// Legacy MVC Controller from online version
    /// </summary>
    public class ResultController
    {
        ////Source: https://stackoverflow.com/questions/483091/how-to-render-an-asp-net-mvc-view-as-a-string 
        //public string RenderRazorViewToString(string viewName, object model)
        //{
        //    ViewData.Model = model;
        //    using (var sw = new StringWriter())
        //    {
        //        var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
        //                                                                 viewName);
        //        var viewContext = new ViewContext(ControllerContext, viewResult.View,
        //                                     ViewData, TempData, sw);
        //        viewResult.View.Render(viewContext, sw);
        //        viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
        //        return sw.GetStringBuilder().ToString();
        //    }
        //}

        //public ActionResult Summary(int id, bool attachCss = false, bool details = false, bool print = false)
        //{
        //    Stopwatch stopWatch = new Stopwatch();
        //    stopWatch.Start();

        //    var _controller = new DamageExtentController();
        //    ProjectResult _result = _controller.computeProjectResult(id, details);

        //    Logging.warn($"ID {id.ToString().PadLeft(4)} - Project Result Summary: elapsed time = " + stopWatch.Elapsed.ToString());
        //    stopWatch.Stop();

        //    ViewBag.attachCss = attachCss;
        //    ViewBag.print = print;

        //    if (_result.ProcessResults.Any())
        //    {
        //        return View(_result);
        //    }
        //    else
        //    {
        //        return View("NoResult", _result);
        //    }
        //}

        //public ActionResult Delete(int id)
        //{
        //    var _controller = new DamageExtentController();
        //    _controller.deleteDamageExtentsFromDB(id);

        //    return Content($"Deleted DamageExtents for project ID {id}");
        //}

        //public ActionResult Create(int id)
        //{
        //    Stopwatch stopWatch = new Stopwatch();
        //    stopWatch.Start();

        //    var potController = new MappedObjectController();
        //    potController.createDamageExtent(id);

        //    Logging.warn($"ID {id.ToString().PadLeft(4)} - Damage Extent Computed: elapsed time = " + stopWatch.Elapsed.ToString());
        //    stopWatch.Stop();

        //    return Content($"Computed DamageExtents for project ID {id}. Elapsed time = " + stopWatch.Elapsed.ToString());
        //}

        //public ActionResult Run(int id, bool attachCss = false)
        //{
        //    Stopwatch stopWatch = new Stopwatch();
        //    stopWatch.Start();

        //    var potController = new MappedObjectController();
        //    potController.createDamageExtent(id);

        //    Logging.warn($"ID {id.ToString().PadLeft(4)} - Damage Extent Computed: elapsed time = " + stopWatch.Elapsed.ToString());
        //    stopWatch.Restart();

        //    var _controller = new DamageExtentController();
        //    ProjectResult _result = _controller.computeProjectResult(id);

        //    Logging.warn($"ID {id.ToString().PadLeft(4)} - Project Result Created: elapsed time = " + stopWatch.Elapsed.ToString());
        //    stopWatch.Stop();

        //    ViewBag.attachCss = attachCss;
        //    return View("Summary", _result);
        //}

        //public PartialViewResult ProcessScenarioChart(List<ScenarioResult> scenarioResults)
        //{
        //    return PartialView(scenarioResults);
        //}

        //public PartialViewResult ProcessSummaryChart(ProcessResult processResult)
        //{
        //    return PartialView(processResult);
        //}

        //public PartialViewResult ProjectChart(ProjectResult projectResult)
        //{
        //    return PartialView(projectResult);
        //}

        //public PartialViewResult SummaryChart(ProjectResult projectResult)
        //{
        //    return PartialView(projectResult);
        //}


        public bool setProjectStatus(int projectId, int statusId)
        {
            if (projectId < 1 || statusId < 1 || statusId > 5)
                return false;

            //update "Project"
            //set "ProjectState_ID" = 1
            //where  "Id" = 9;

            string _setProjectStatusString =
                $"update \"Project\" " +
                $"set \"ProjectState_ID\" = {statusId} " +
                $"where \"Id\"={projectId} ";

            using (var transaction = DBManager.ActiveSession.BeginTransaction())
            {
                ISQLQuery query = DBManager.ActiveSession.CreateSQLQuery(_setProjectStatusString);
                query.ExecuteUpdate();
                DBManager.ActiveSession.Flush();
                transaction.Commit();
            }

            return true;
        }

    }
}