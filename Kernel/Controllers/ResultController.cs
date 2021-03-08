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