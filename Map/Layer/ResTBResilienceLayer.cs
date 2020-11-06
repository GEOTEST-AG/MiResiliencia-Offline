using AxMapWinGIS;
using MapWinGIS;
using ResTB.DB.Models;
using ResTB.Map.Layer;
using ResTB.Map.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Layer
{

    public class ResTBResilienceLayer : ResTBPostGISLayer
    {
        public ResTBResilienceLayer(int Project, bool IsBeforeMitigationMeasure) : base()
        {
            this.Project = Project;
            if (IsBeforeMitigationMeasure) ResTBPostGISType = ResTBPostGISType.ResilienceBefore;
            else ResTBPostGISType = ResTBPostGISType.ResilienceAfter;
            this.SQL = "select * from \"MappedObjectWithResilience\" where \"Project_Id\" = " + Project;
            if (IsBeforeMitigationMeasure) Name = ResTB.Translation.Properties.Resources.Resilience_before_Measure;
            else Name = ResTB.Translation.Properties.Resources.Resilience_after_Measure;
            ExportImportFileName = "Resilience";
            SQL_Layer = "MappedObject";
            ProjectID = "Project_Id";
            VisibilityExpression = "[" + ProjectID + "] = " + Project;
            Style = new Style.ResilienceStyle();
        }
        
        public override object GetObjectFromShape(Shapefile shapefile, int index)
        {
            var myId = shapefile.FieldIndexByName["mappedObjectID"];

            int moId = shapefile.CellValue[myId, index];

            // Search the corresponding maphazard Object
            using (DB.ResTBContext db = new DB.ResTBContext())
            {
                MappedObject mo = db.MappedObjects.AsNoTracking().Where(m => m.ID == moId).FirstOrDefault();
                return mo;
            }
        }

    }
}