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

    public class ResTBDamagePotentialLayer : ResTBPostGISLayer
    {
        public Objectparameter CurrentObjectparameter { get; set; }


        public string SQL_Point { get { return "select * from \"MappedObjectPoint\" where \"Project_Id\" = " + Project; } }
        public string SQL_Line { get { return "select * from \"MappedObjectLine\" where \"Project_Id\" = " + Project; } }
        public string SQL_Polygon { get { return "select * from \"MappedObjectPolygon\" where \"Project_Id\" = " + Project; } }

        public int PointHandle { get; set; }
        public int LineHandle { get; set; }
        public int PolygonHandle { get; set; }

        private int _current_handle = -1;

        public new int Handle
        {
            get
            {
                if (_current_handle < 0) _current_handle = PointHandle;
                return _current_handle;
            }
            set
            {
                _current_handle = value;
            }
        }

        public ResTBDamagePotentialLayer(int Project) : base()
        {
            this.Project = Project;
            LayerType = LayerType.ProjectLayer;
            ResTBPostGISType = ResTBPostGISType.DamagePotential;
            this.Name = Translation.Properties.Resources.DamagePotential;
            ExportImportFileName = "DamagePotential";
            SQL_Layer = "MappedObject";
            ProjectID = "Project_Id";
            VisibilityExpression = "[Project_Id] = " + Project;

            Style = new Style.DamagePotentialStyle();
        }


        public override void SaveAttributes(AxMap axMap)
        {
            // check if it is a new shape and we have a reference to the project.
            var editingLayer = axMap.get_Shapefile(EditingLayerHandle);
            var project_index = editingLayer.FieldIndexByName[ProjectID];
            var objectparameter_index = editingLayer.FieldIndexByName["Objectparameter_ID"];

            for (int i = 0; i < editingLayer.NumShapes; i++)
            {
                if (editingLayer.CellValue[project_index, i] == null)
                {
                    editingLayer.EditCellValue(project_index, i, Project);
                    editingLayer.EditCellValue(objectparameter_index, i, CurrentObjectparameter.ID);
                }
            }

        }

        public override void ApplyStyle(AxMap AxMap)
        {
            Style.setStyleForLayer(AxMap, this.PointHandle);
            Style.setStyleForLayer(AxMap, this.LineHandle);
            Style.setStyleForLayer(AxMap, this.PolygonHandle);

        }

        public override object GetObjectFromShape(Shapefile shapefile, int index)
        {
            var myId = shapefile.FieldIndexByName["ID"];

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
