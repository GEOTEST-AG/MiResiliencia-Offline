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
using System.Windows.Shapes;

namespace ResTB.Map.Layer
{

    public class ResTBMitigationMeasureLayer : ResTBPostGISLayer
    {
        public ResTBMitigationMeasureLayer(int Project) : base()
        {
            this.Project = Project;
            ResTBPostGISType = ResTBPostGISType.MitigationMeasure;
            this.SQL = "select * from \"ProtectionMeasureGeometry\" where \"project_fk\" = " + Project;
            Name = ResTB.Translation.Properties.Resources.MitigationMeasure;
            SQL_Layer = "ProtectionMeasureGeometry";
            ExportImportFileName = "MitigationMeasure";
            ProjectID = "project_fk";
            VisibilityExpression = "[" + ProjectID + "] = " + Project;
            Style = new Style.MitigationStyle();
        }

        public override object GetObjectFromShape(Shapefile shapefile, int index)
        {
            // give back the Mitigation Obejct. We habe only one for all shapes
            using (DB.ResTBContext db = new DB.ResTBContext())
            {
                ProtectionMeasure pm = db.ProtectionMeasurements.AsNoTracking().Where(m => m.Project.Id == Project).FirstOrDefault();
                return pm;
            }
        }

    }
}