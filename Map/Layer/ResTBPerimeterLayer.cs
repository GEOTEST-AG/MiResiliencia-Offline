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

    public class ResTBPerimeterLayer : ResTBPostGISLayer
    {
        public ResTBPerimeterLayer(int Project) : base()
        {
            this.Project = Project;
            ResTBPostGISType = ResTBPostGISType.Perimeter;
            this.SQL = "select * from \"Perimeter\" where \"project_fk\" = " + Project;
            Name = ResTB.Translation.Properties.Resources.Perimeter;
            ExportImportFileName = "Perimeter";
            SQL_Layer = "Perimeter";
            ProjectID = "project_fk";
            VisibilityExpression = "[" + ProjectID + "] = " + Project;
            Style = new Style.PerimeterStyle();
        }

        public override object GetObjectFromShape(Shapefile shapefile, int index)
        {
            // give back the Mitigation Obejct. We habe only one for all shapes
            using (DB.ResTBContext db = new DB.ResTBContext())
            {
                Project p = db.Projects.AsNoTracking().Where(m => m.Id == Project).FirstOrDefault();
                return p;
            }
        }

    }
}