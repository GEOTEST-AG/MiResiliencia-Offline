using AxMapWinGIS;
using MapWinGIS;
using ResTB.DB.Models;
using ResTB.Map.Layer;
using ResTB.Map.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Layer
{

    public class ResTBHazardMapLayer : ResTBPostGISLayer
    {
        public NatHazard NatHazard { get; set; }
        public int Index { get; set; }

        public ResTBHazardMapLayer(int Project, bool IsBeforeMitigationMeasure, NatHazard NatHazard, int Index) : base()
        {
            this.Project = Project;
            this.NatHazard = NatHazard;
            this.Index = Index;
            LayerType = LayerType.ProjectLayer;
            if (IsBeforeMitigationMeasure) ResTBPostGISType = ResTBPostGISType.HazardMapBefore;
            else ResTBPostGISType = ResTBPostGISType.HazardMapAfter;
            ExportImportFileName = "HazardMap";
            this.SQL = "select * from \"HazardMap\" where \"Project_Id\" = " + Project + " and \"NatHazard_ID\" = " + NatHazard.ID + " and \"BeforeAction\" = " + IsBeforeMitigationMeasure + " and \"Index\" = " + Index;
            SQL_Layer = "HazardMap";
            ProjectID = "Project_Id";
            if (IsBeforeMitigationMeasure) VisibilityExpression = "[Project_Id] = " + Project + " AND [NatHazard_ID] = " + NatHazard.ID + " AND [Index] = " + Index + " AND [BeforeAction] = 1";
            else VisibilityExpression = "[Project_Id] = " + Project + " AND [NatHazard_ID] = " + NatHazard.ID + " AND [Index] = " + Index + " AND [BeforeAction] = 0";

            Style = new Style.HazardMapStyle();
        }


        public override void SaveAttributes(AxMap axMap)
        {
            // check if it is a new shape and we have a reference to the project.
            var editingLayer = axMap.get_Shapefile(EditingLayerHandle);
            var project_index = editingLayer.FieldIndexByName[ProjectID];
            var before_index = editingLayer.FieldIndexByName["BeforeAction"];
            var index_index = editingLayer.FieldIndexByName["Index"];
            var nathazard_index = editingLayer.FieldIndexByName["NatHazard_ID"];

            for (int i = 0; i < editingLayer.NumShapes; i++)
            {
                if (editingLayer.CellValue[project_index, i] == null)
                {
                    editingLayer.EditCellValue(project_index, i, Project);
                    if (ResTBPostGISType == ResTBPostGISType.HazardMapBefore) editingLayer.EditCellValue(before_index, i, 1);
                    else if (ResTBPostGISType == ResTBPostGISType.HazardMapAfter) editingLayer.EditCellValue(before_index, i, 0);
                    editingLayer.EditCellValue(index_index, i, Index);
                    editingLayer.EditCellValue(nathazard_index, i, NatHazard.ID);

                }
            }



        }

        public string CopyToAfter(MapControlTools mapControlTools)
        {
            // copy the current hazard map to hazardmapafter
            using (DB.ResTBContext db = new DB.ResTBContext())
            {
                HazardMap hm = db.HazardMaps.Include(m=>m.Project).Include(m=>m.NatHazard).OrderByDescending(u => u.ID).FirstOrDefault();
                if (hm.Project.Id == Project)
                {
                    string copySQL = "insert into \"HazardMap\" (\"Index\",\"BeforeAction\", \"NatHazard_ID\" , \"Project_Id\" , geometry ) " +
    "select \"Index\" , false, \"NatHazard_ID\" , \"Project_Id\" , geometry from " +
    "\"HazardMap\" where \"ID\" = " + hm.ID;
                    db.Database.ExecuteSqlCommand(copySQL);


                    ResTBHazardMapLayer hazard = new ResTBHazardMapLayer(
                                    Project, false, NatHazard, Index);

                    if (!mapControlTools.Layers.Where(m => m.Name.Equals(hazard.Name)).Any())
                    {
                        mapControlTools.AddProjectLayer(hazard);
                    }

                    return hazard.Name;
                }

            }
            return "";

        }

        public override object GetObjectFromShape(Shapefile shapefile, int index)
        {
            var myId = shapefile.FieldIndexByName["ID"];

            int hmId = shapefile.CellValue[myId, index];

            // Search the corresponding maphazard Object
            using (DB.ResTBContext db = new DB.ResTBContext())
            {
                HazardMap hm = db.HazardMaps.AsNoTracking().Where(m => m.ID == hmId).FirstOrDefault();
                return hm;
            }
        }

    }
}
