using AxMapWinGIS;
using MapWinGIS;
using ResTB.Map.Style;
using System;

namespace ResTB.Map.Layer
{
    public enum ResTBPostGISType
    {
        Perimeter = 0,
        HazardMapBefore = 1,
        DamagePotential = 2,
        ResilienceBefore = 3,
        MitigationMeasure = 4,
        HazardMapAfter = 5,
        ResilienceAfter = 6,
        //Result = 7, //not in use
        RiskMap = 8,
        RiskMapAfter = 9
    }

    public class ResTBPostGISLayer : BaseLayer, ILayer
    {

        private string _sql = "";
        public string SQL
        {
            get
            {
                if (_sql != "") return _sql;
                if (Project == 0) throw new Exception("Missing project number");

                string sql = "";
                return sql;

            }
            set
            {
                _sql = value;
            }
        }

        private string _name = "";
        public string Name
        { get
            {
                if (_name != "") return _name;
                ResTBHazardMapLayer hazard;
                switch (ResTBPostGISType)
                {
                    case ResTBPostGISType.HazardMapBefore:
                        hazard = (ResTBHazardMapLayer)this;
                        return "Hazardmap before " + hazard.NatHazard.Name + " (" + hazard.Index + ")";
                        break;
                    case ResTBPostGISType.HazardMapAfter:
                        hazard = (ResTBHazardMapLayer)this;
                        return "Hazardmap after " + hazard.NatHazard.Name + " (" + hazard.Index + ")";
                        break;
                    default:
                        return "Not defined";
                        break;

                }
            }
            set { _name = value; }
        }


        private string _sql_Layer="";

        public string SQL_Layer
        {
            get
            {
                if (_sql_Layer != "") return _sql_Layer;
                if (Project == 0) throw new Exception("Missing project number");

                string sql_Layer = "";
                return sql_Layer;

            }
            set
            {
                _sql_Layer = value;
            }
        }

        public int Project { get; set; }
        public ResTBPostGISType ResTBPostGISType { get; set; }

        public OgrLayer EditingLayer { get; set; }
        public int EditingLayerHandle { get; set; }

        private string _projectID = "";
        public string ProjectID
        {
            get
            {
                if (_projectID != "") return _projectID;
                
                return _projectID;
            }
            set
            {
                _projectID = value;
            }
        }

        private string _visibilityExpression = "";
        public string VisibilityExpression { get
            {
                if (_visibilityExpression != "") return _visibilityExpression;
                if (Project == 0) throw new Exception("Missing project number");

                string visibilityExpression = "";

                return visibilityExpression;
            }
            set { _visibilityExpression = value; }

        }
        private Style.IStyle _style;
        public IStyle Style
        {
            get
            {
                if (_style != null) return _style;
                return null;

            }
            set { _style = value; }
        }

        public ResTBPostGISLayer()
        {
            LayerType = LayerType.ProjectLayer;
        }


        public override void ApplyStyle(AxMap AxMap)
        {
            Style.setStyleForLayer(AxMap, this.Handle);
            
        }


        public virtual void SaveAttributes(AxMap axMap)
        {
            // check if it is a new shape and we have a reference to the project.
            var editingLayer = axMap.get_Shapefile(EditingLayerHandle);
            var project_index = editingLayer.FieldIndexByName[ProjectID];
            for (int i = 0; i < editingLayer.NumShapes; i++)
            {
                if (editingLayer.CellValue[project_index, i] == null) editingLayer.EditCellValue(project_index, i, Project);
            }

        }

    }
}
