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

    public class ResTBRiskMapLayer : ResTBPostGISLayer
    {

        public string SQL_Point
        {
            get
            {
                if (BeforeProtectionMeasure) return "select * from restb_get_riskmap_point(" + Project + ",true)";
                return "select * from restb_get_riskmap_point(" + Project + ",false)";
            }
        }
        public string SQL_Line
        {
            get
            {
                if (BeforeProtectionMeasure) return "select * from restb_get_riskmap_line(" + Project + ",true)";
                else return "select * from restb_get_riskmap_line(" + Project + ",false)";
            }
        }
        public string SQL_Polygon
        {
            get
            {
                if (BeforeProtectionMeasure) return "select * from restb_get_riskmap_polygon(" + Project + ",true)";
                else return "select * from restb_get_riskmap_polygon(" + Project + ",false)";
            }
        }

        public int PointHandle { get; set; }
        public int LineHandle { get; set; }
        public int PolygonHandle { get; set; }

        public bool BeforeProtectionMeasure { get; set; }

        public new int Handle
        {
            get
            {
                return PointHandle;

            }
        }

        public ResTBRiskMapLayer(int Project, bool BeforeProtectionMeasure = true) : base()
        {
            this.Project = Project;
            LayerType = LayerType.ProjectLayer;
            if (BeforeProtectionMeasure == false)
            {
                ResTBPostGISType = ResTBPostGISType.RiskMapAfter;
                this.Name = Translation.Properties.Resources.RiskMapWithMitigationMeasure;
            }
            else
            {
                ResTBPostGISType = ResTBPostGISType.RiskMap;
                this.Name = Translation.Properties.Resources.RiskMapWithoutMitigationMeasure;
            }
            ExportImportFileName = "RiskMap";
            SQL_Layer = "";
            ProjectID = "Project_Id";
            VisibilityExpression = "";
            this.BeforeProtectionMeasure = BeforeProtectionMeasure;

            Style = new Style.RiskMapStyle();
        }


        public override void ApplyStyle(AxMap AxMap)
        {
            Style.setStyleForLayer(AxMap, this.PointHandle);
            Style.setStyleForLayer(AxMap, this.LineHandle);
            Style.setStyleForLayer(AxMap, this.PolygonHandle);
        }


    }
}
