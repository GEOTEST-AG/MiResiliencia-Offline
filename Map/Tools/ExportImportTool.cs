using MapWinGIS;
using ResTB.DB.Models;
using ResTB.Map.Layer;
using ResTB.Translation.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResTB.Map.Tools
{
    public class ExportImportTool : BaseTool
    {
        public int openTasks;
        public int finishedTasks;

        private string localDirectory;

        public ExportImportTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTool) : base(axMap, mapControlTool)
        {
            string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!System.IO.Directory.Exists(localData + "\\ResTBDesktop")) System.IO.Directory.CreateDirectory(localData + "\\ResTBDesktop");

            localDirectory = localData + "\\ResTBDesktop";
        }

        public bool ExportProject(int Project, string filename)
        {
            try
            {
                if (System.IO.Directory.Exists(localDirectory + "\\ExportMap")) DeleteDirectory(localDirectory + "\\ExportMap");
                System.IO.Directory.CreateDirectory(localDirectory + "\\ExportMap");

                Thread exportProjectThread = new Thread(() => ExportProjectThread(Project, filename));

                Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProject";
                bc.Percent = 0;
                bc.Message = Resources.Project_Export;

                MapControlTools.On_BusyStateChange(bc);

                exportProjectThread.Start();
                return true;
            }
            catch (Exception e)
            {
                Events.MapControl_Error export_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.ImportExportError, InMethod = "ExportProject", AxMapError = e.ToString() };
                On_Error(export_error);
                return false;
            }
        }

        public int ImportProject(string filename)
        {
            try
            {
                Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ImportProject";
                bc.Percent = 0;
                bc.Message = Resources.Project_Import;
                MapControlTools.On_BusyStateChange(bc);

                if (System.IO.Directory.Exists(localDirectory + "\\ImportMap")) DeleteDirectory(localDirectory + "\\ImportMap");
                System.IO.Directory.CreateDirectory(localDirectory + "\\ImportMap");
                
                MapControlTools.On_BusyStateChange(bc);

                return ImportProjectThread(filename);

            }
            catch (Exception e)
            {
                Events.MapControl_Error export_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.ImportExportError, InMethod = "ImportProject", AxMapError = e.ToString() };
                On_Error(export_error);
            }
            return -1;
        }


        #region ExportFunctions
        private bool RunGdalTranslate(string inputFilename, string outputFilename, string outputType, string sql, bool append = false, string newTableName=null)
        {
            var gdalUtils = new GdalUtils();
            var options = new[]
                {
                    "-f", outputType,
                    "-sql", sql, "-overwrite"
                };

            if (append)
            {
                List<string> newOptions = options.ToList();
                newOptions.Add("-append");
                options = newOptions.ToArray();
            }
            if (newTableName!=null)
            {
                List<string> newOptions = options.ToList();
                newOptions.Add("-nln");
                newOptions.Add(newTableName);
                options = newOptions.ToArray();
            }

            ExportImportCallback callback = new ExportImportCallback(MapControlTools);
            callback.GdalFinished += Callback_GdalFinished;
            gdalUtils.GlobalCallback = callback;
            openTasks++;
            if (!gdalUtils.GdalVectorTranslate(inputFilename, outputFilename, options, true))
            {
                Events.MapControl_Error export_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.ImportExportError, InMethod = "ExportProject", AxMapError = gdalUtils.ErrorMsg[gdalUtils.LastErrorCode] + " Detailed error: " + gdalUtils.DetailedErrorMsg };
                On_Error(export_error);

                return false;
            }
            return true;
        }

        private void Callback_GdalFinished(object sender, EventArgs e)
        {
            finishedTasks++;
        }

        private bool ExportProjectLayer(ResTBPostGISLayer projectLayer)
        {
            if (projectLayer == null) return true;
            DBConnectionTool db = new DBConnectionTool(this.AxMap, this.MapControlTools);
            
            string outputFilename = localDirectory + "\\ExportMap\\" + projectLayer.ExportImportFileName + ".shp";
            string inputFilename = db.GetGdalConnectionString();

            if (projectLayer.GetType()==typeof(ResTBDamagePotentialLayer))
            {
                if (!RunGdalTranslate(inputFilename, localDirectory + "\\ExportMap\\" + projectLayer.ExportImportFileName + "_Points.shp", "ESRI Shapefile", "select \"ID\", \"FreeFillParameter_ID\", \"Objectparameter_ID\", \"Project_Id\", point from \"MappedObject\" where point is not null and \"Project_Id\" = "+ projectLayer.Project)) return false;
                if (!RunGdalTranslate(inputFilename, localDirectory + "\\ExportMap\\" + projectLayer.ExportImportFileName + "_Lines.shp", "ESRI Shapefile", "select \"ID\", \"FreeFillParameter_ID\", \"Objectparameter_ID\", \"Project_Id\", line from \"MappedObject\" where line is not null and \"Project_Id\" = " + projectLayer.Project)) return false;
                if (!RunGdalTranslate(inputFilename, localDirectory + "\\ExportMap\\" + projectLayer.ExportImportFileName + "_Polygones.shp", "ESRI Shapefile", "select \"ID\", \"FreeFillParameter_ID\", \"Objectparameter_ID\", \"Project_Id\", polygon from \"MappedObject\" where polygon is not null and \"Project_Id\" = " + projectLayer.Project)) return false;
                return true;
            }
            else if (projectLayer.GetType() == typeof(ResTBHazardMapLayer))
            {
                if (!RunGdalTranslate(inputFilename, localDirectory + "\\ExportMap\\" + projectLayer.ExportImportFileName + ".shp", "ESRI Shapefile", "select * from \"HazardMap\" where \"Project_Id\" =" + projectLayer.Project)) return false;
                 return true;
            }
            return RunGdalTranslate(inputFilename, outputFilename, "ESRI Shapefile", projectLayer.SQL);
        }

        private void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        
        public void ExportProjectThread(int Project, string filename)
        {
            try
            {
                Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 0;
                bc.Message = Resources.MapControl_Exporting_Layers;
                MapControlTools.On_BusyStateChange(bc);

                if (!ExportProjectLayer((ResTBPostGISLayer)MapControlTools.Layers.Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.Perimeter).FirstOrDefault())) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 10;
                bc.Message = Resources.MapControl_Exporting_Layers + "(" + Resources.Perimeter + ")";
                MapControlTools.On_BusyStateChange(bc);
                if (!ExportProjectLayer((ResTBPostGISLayer)MapControlTools.Layers.Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.MitigationMeasure).FirstOrDefault())) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 20;
                bc.Message = Resources.MapControl_Exporting_Layers + "(" + Resources.MitigationMeasure + ")";
                MapControlTools.On_BusyStateChange(bc);
                if (!ExportProjectLayer((ResTBPostGISLayer)MapControlTools.Layers.Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.DamagePotential).FirstOrDefault())) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 30;
                bc.Message = Resources.MapControl_Exporting_Layers + "(" + Resources.DamagePotential + ")";
                MapControlTools.On_BusyStateChange(bc);
                if (!ExportProjectLayer((ResTBPostGISLayer)MapControlTools.Layers.Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.HazardMapBefore).FirstOrDefault())) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 40;
                bc.Message = Resources.MapControl_Exporting_Layers + "(" + Resources.Hazard_Maps_before_Measure + ")";
                MapControlTools.On_BusyStateChange(bc);
                if (!ExportProjectLayer((ResTBPostGISLayer)MapControlTools.Layers.Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.HazardMapAfter).FirstOrDefault())) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 50;
                bc.Message = Resources.MapControl_Exporting_Layers + "(" + Resources.Hazard_Maps_after_Measure + ")";
                MapControlTools.On_BusyStateChange(bc);
                DBConnectionTool db = new DBConnectionTool(this.AxMap, this.MapControlTools);

                if (!RunGdalTranslate(db.GetGdalConnectionString(), localDirectory + "\\ExportMap\\database.sqlite", "SQLite", "select * from \"Project\" where \"Id\" = " + Project, false, "Project")) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 60;
                bc.Message = Resources.MapControl_Exporting_Database;
                MapControlTools.On_BusyStateChange(bc);

                if (!RunGdalTranslate(db.GetGdalConnectionString(), localDirectory + "\\ExportMap\\database.sqlite", "SQLite", "select * from \"ProtectionMeasure\" where \"ProjectID\" = " + Project, true, "ProtectionMeasure")) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 70;
                bc.Message = Resources.MapControl_Exporting_Database;
                MapControlTools.On_BusyStateChange(bc);
                
                if (!RunGdalTranslate(db.GetGdalConnectionString(), localDirectory + "\\ExportMap\\database.sqlite", "SQLite", "select o.* from \"Objectparameter\" o inner join \"MappedObject\" mo on (mo.\"FreeFillParameter_ID\"  = o.\"ID\" ) where mo.\"Project_Id\" = " + Project + " union select o.* from \"Objectparameter\" o inner join \"MappedObject\" mo on (mo.\"Objectparameter_ID\"  = o.\"ID\" ) where o.\"IsStandard\" = false and mo.\"Project_Id\" = " + Project, true, "Objectparameter")) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 70;
                bc.Message = Resources.MapControl_Exporting_Database;
                MapControlTools.On_BusyStateChange(bc);

                if (!RunGdalTranslate(db.GetGdalConnectionString(), localDirectory + "\\ExportMap\\database.sqlite", "SQLite", "select o.* from \"ResilienceValues\" o inner join \"MappedObject\" mo on (mo.\"ID\"  = o.\"MappedObject_ID\" ) where mo.\"Project_Id\" = " + Project , true, "ResilienceValues")) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 70;
                bc.Message = Resources.MapControl_Exporting_Database;
                MapControlTools.On_BusyStateChange(bc);

                if (!RunGdalTranslate(db.GetGdalConnectionString(), localDirectory + "\\ExportMap\\database.sqlite", "SQLite", "select * from \"PrA\" where \"ProjectId\" = " + Project, true, "PrA")) return;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 70;
                bc.Message = Resources.MapControl_Exporting_Database;
                MapControlTools.On_BusyStateChange(bc);

                while (openTasks != finishedTasks) { }


                // Now Zip it...
                if (System.IO.File.Exists(filename)) System.IO.File.Delete(filename);
                ZipFile.CreateFromDirectory(localDirectory + "\\ExportMap", filename);
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Idle;
                bc.KeyOfSender = "ExportProjectThread";
                bc.Percent = 100;
                bc.Message = Resources.MapControl_Exporting_ExportetTo + filename;
                MapControlTools.On_BusyStateChange(bc);

                DeleteDirectory(localDirectory + "\\ExportMap");
            }
            catch (Exception e)
            {
                Events.MapControl_Error export_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.ImportExportError, InMethod = "ExportProject", AxMapError = e.ToString() };
                On_Error(export_error);
            }

            return;
        }
        #endregion ExportFunctions

        #region ImportFunctions

        private bool RunGdalImport(string inputFilename, string newTableName = null, string tablename = null)
        {
            DBConnectionTool db = new DBConnectionTool(this.AxMap, this.MapControlTools);
            string outputFilename = db.GetGdalConnectionString();

            var gdalUtils = new GdalUtils();
            var options = new[] { "-append" };
            if ((newTableName!=null)) options = new[]
                { "-nln", newTableName, "-overwrite", tablename
                };
            

            ExportImportCallback callback = new ExportImportCallback(MapControlTools);
            callback.GdalFinished += Callback_GdalFinished;
            gdalUtils.GlobalCallback = callback;
            openTasks++;
            if (!gdalUtils.GdalVectorTranslate(inputFilename, outputFilename, options, true))
            {
                Events.MapControl_Error export_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.ImportExportError, InMethod = "ImportProject", AxMapError = gdalUtils.ErrorMsg[gdalUtils.LastErrorCode] + " Detailed error: " + gdalUtils.DetailedErrorMsg };
                On_Error(export_error);
                return false;
            }
            return true;
        }

        private TEntity ShallowCopyEntity<TEntity>(TEntity source) where TEntity : class, new()
        {

            // Get properties from EF that are read/write and not marked witht he NotMappedAttribute
            var sourceProperties = typeof(TEntity)
                                    .GetProperties()
                                    .Where(p => p.CanRead && p.CanWrite &&
                                                p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), true).Length == 0);
            var newObj = new TEntity();

            foreach (var property in sourceProperties)
            {

                // Copy value
                property.SetValue(newObj, property.GetValue(source, null), null);
                

            }

            return newObj;

        }


        public int ImportProjectThread(string filename)
        {
            try
            {
                ZipFile.ExtractToDirectory(filename, localDirectory + "\\ImportMap");

                // import the project, add it to the database as a new project with new id
                if (File.Exists(localDirectory + "\\ImportMap\\database.sqlite")) RunGdalImport(localDirectory + "\\ImportMap\\database.sqlite", "projectimport","project");
                using (var context = new DB.ResTBContext())
                {
                    List<Project> importproject = context.Projects
                                        .SqlQuery("Select * from projectimport")
                                        .ToList<Project>();

                    if ((importproject!=null) && (importproject.Count>0))
                    {
                        Project p = new Project();
                        p.Description = importproject[0].Description;
                        p.CoordinateSystem = importproject[0].CoordinateSystem;

                        // check if name is already there...
                        string name = importproject[0].Name;
                        Project pold = context.Projects.Where(m => m.Name == name).FirstOrDefault();
                        if (pold != null) name = name + " (import " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +")";


                        p.Name = name;
                        p.Number = importproject[0].Number ;
                        context.Projects.Add(p);
                        context.SaveChanges();

                        Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
                        bc.BusyState = Events.BusyState.Busy;
                        bc.KeyOfSender = "ImportProject";
                        bc.Percent = 10;
                        bc.Message = Resources.MapControl_Importing;
                        MapControlTools.On_BusyStateChange(bc);

                        // we have a new project id

                        //import all new Objectparameters
                        RunGdalImport(localDirectory + "\\ImportMap\\database.sqlite", "objectparameterimport", "objectparameter");

                        RunGdalImport(localDirectory + "\\ImportMap\\database.sqlite", "resiliencevaluesimport", "resiliencevalues");

                        RunGdalImport(localDirectory + "\\ImportMap\\database.sqlite", "praimport", "pra");
                        List<PrA> importpra = context.PrAs
                                        .SqlQuery("Select * from praimport")
                                        .ToList<PrA>();
                        foreach (PrA praImp in importpra)
                        {
                            PrA pra = (PrA)praImp.Clone();
                            pra.Project = p;
                            p.PrAs.Add(pra);
                            context.PrAs.Add(pra);
                        }

                        context.SaveChanges();

                        RunGdalImport(localDirectory + "\\ImportMap\\database.sqlite", "protectionmeasureimport", "ProtectionMeasure");
                        List<ProtectionMeasure> importpm = context.ProtectionMeasurements
                                        .SqlQuery("Select * from protectionmeasureimport")
                                        .ToList<ProtectionMeasure>();
                        if (importpm?.Count>0)
                        {
                            ProtectionMeasure pm = (ProtectionMeasure)importpm[0].Clone();
                            pm.Project = p;
                            p.ProtectionMeasure = pm;
                            context.ProtectionMeasurements.Add(pm);
                            context.SaveChanges();
                        }

                        // create the ids of old objectparameters, so foreign constraint will work.
                        // But don't override existing
                        List<Objectparameter> importop = context.Objektparameter
                                        .SqlQuery("Select * from objectparameterimport")
                                        .ToList<Objectparameter>();

                        // 
                        foreach (Objectparameter o in importop)
                        {
                            if (context.Objektparameter.Where(m=>m.ID==o.ID).ToList().Count == 0) context.Database.ExecuteSqlCommand("INSERT INTO public.\"Objectparameter\" (\"ID\",\"FeatureType\",\"Value\",\"Floors\",\"Personcount\",\"Presence\",\"NumberOfVehicles\",\"Velocity\",\"Staff\",\"IsStandard\",\"ObjectClass_ID\")\tVALUES (" + o.ID + ",1,0,0,0,0.0,0,0,0,false,1); ");
                        }



                        ResTBPostGISLayer tempLayer = new ResTBMitigationMeasureLayer(0);                        
                        if (File.Exists(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + ".shp"))
                        {
                            RunGdalImport(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + ".shp", "protectionmeasuregeometryimport");
                            context.Database.ExecuteSqlCommand("insert into \"ProtectionMeasureGeometry\" (project_fk, geometry) select " + p.Id + ", wkb_geometry from protectionmeasuregeometryimport;");
                        }
                        bc = new Events.MapControl_BusyStateChange();
                        bc.BusyState = Events.BusyState.Busy;
                        bc.KeyOfSender = "ImportProject";
                        bc.Percent = 30;
                        bc.Message = Resources.MapControl_Importing;
                        MapControlTools.On_BusyStateChange(bc);

                        tempLayer = new ResTBPerimeterLayer(0);
                        if (File.Exists(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + ".shp"))
                        {
                            RunGdalImport(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + ".shp", "perimeterimport");
                            context.Database.ExecuteSqlCommand("insert into \"Perimeter\" (project_fk, geometry) select " + p.Id + ", wkb_geometry from perimeterimport;");
                        }
                        bc = new Events.MapControl_BusyStateChange();
                        bc.BusyState = Events.BusyState.Busy;
                        bc.KeyOfSender = "ImportProject";
                        bc.Percent = 40;
                        bc.Message = Resources.MapControl_Importing;
                        MapControlTools.On_BusyStateChange(bc);

                        tempLayer = new ResTBHazardMapLayer(0,false,context.NatHazards.ToList().First(),0);
                        if (File.Exists(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + ".shp"))
                        {
                            RunGdalImport(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + ".shp", "hazardmapimport");
                            context.Database.ExecuteSqlCommand("insert into \"HazardMap\" (\"Project_Id\", \"Index\",\"BeforeAction\",\"NatHazard_ID\",geometry) select " + p.Id + ",\"index\",CASE WHEN beforeacti IS NULL THEN false WHEN beforeacti = 0 THEN false ELSE true END beforeaction ,\"nathazard_\", wkb_geometry from hazardmapimport;");
                        }
                        bc = new Events.MapControl_BusyStateChange();
                        bc.BusyState = Events.BusyState.Busy;
                        bc.KeyOfSender = "ImportProject";
                        bc.Percent = 50;
                        bc.Message = Resources.MapControl_Importing;
                        MapControlTools.On_BusyStateChange(bc);

                        tempLayer = new ResTBDamagePotentialLayer(0);
                        if (File.Exists(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + "_Points.shp"))
                        {
                            RunGdalImport(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + "_Points.shp", "damagepotentialpoints");
                            context.Database.ExecuteSqlCommand("insert into \"MappedObject\" (\"Project_Id\", \"FreeFillParameter_ID\",\"Objectparameter_ID\",point) select "+p.Id+", freefillpa, objectpara, wkb_geometry from damagepotentialpoints;");
                        }
                        if (File.Exists(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + "_Lines.shp"))
                        {
                            RunGdalImport(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + "_Lines.shp", "damagepotentiallines");
                            context.Database.ExecuteSqlCommand("insert into \"MappedObject\" (\"Project_Id\", \"FreeFillParameter_ID\",\"Objectparameter_ID\",line) select " + p.Id + ", freefillpa, objectpara, wkb_geometry from damagepotentiallines;");
                        }

                        List<int> importrvsMOID = context.Database.SqlQuery<int>("select distinct mappedobject_id from resiliencevaluesimport")
                                        .ToList<int>();

                       
                        bc = new Events.MapControl_BusyStateChange();
                        bc.BusyState = Events.BusyState.Busy;
                        bc.KeyOfSender = "ImportProject";
                        bc.Percent = 60;
                        bc.Message = Resources.MapControl_Importing;
                        MapControlTools.On_BusyStateChange(bc);




                       
                        if (File.Exists(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + "_Polygones.shp"))
                        {
                            RunGdalImport(localDirectory + "\\ImportMap\\" + tempLayer.ExportImportFileName + "_Polygones.shp", "damagepotentialpolygones");
                            context.Database.ExecuteSqlCommand("insert into \"MappedObject\" (\"Project_Id\", \"FreeFillParameter_ID\",\"Objectparameter_ID\",polygon) select " + p.Id + ", freefillpa, objectpara, wkb_geometry from damagepotentialpolygones;");
                        }
                        bc = new Events.MapControl_BusyStateChange();
                        bc.BusyState = Events.BusyState.Busy;
                        bc.KeyOfSender = "ImportProject";
                        bc.Percent = 70;
                        bc.Message = Resources.MapControl_Importing;
                        MapControlTools.On_BusyStateChange(bc);

                        foreach (int moID in importrvsMOID)
                        {
                            string sql = "insert into \"ResilienceValues\" (\"OverwrittenWeight\", \"Value\", \"MappedObject_ID\", \"ResilienceWeight_ID\") " +
"select r.overwrittenweight, r.value, (select \"ID\" " +
"from \"MappedObject\" mo where polygon = ( " +
"select wkb_geometry from damagepotentialpolygones d " +
"where d.id = " + moID + ") " +
"and \"Project_Id\" = " + p.Id + "), r.resilienceweight_id from resiliencevaluesimport r where r.mappedobject_id = " + moID + "; ";
                            context.Database.ExecuteSqlCommand(sql);
                        }



                        List<MappedObject> mos = context.MappedObjects.Where(m => m.Project.Id == p.Id).ToList();

                        // Insert FreeFills and Objectparameters
                        
                        foreach (MappedObject mo in mos)
                        {
                            if (mo.FreeFillParameter != null)
                            {
                                Objectparameter o = importop.Where(m => m.ID == mo.FreeFillParameter.ID).FirstOrDefault();
                                Objectparameter oCopy = ShallowCopyEntity<Objectparameter>(o);
                                context.Objektparameter.Add(oCopy);
                                mo.FreeFillParameter = oCopy;
                                context.SaveChanges();
                            }
                            if (mo.Objectparameter != null)
                            {
                                Objectparameter o = importop.Where(m => m.ID == mo.Objectparameter.ID).FirstOrDefault();
                                
                                // not a standard objectparameter
                                if (o != null)
                                {
                                    Objectparameter oCopy = ShallowCopyEntity<Objectparameter>(o);

                                    // get the objectclass
                                    int objClassID = context.Database.SqlQuery<int>("select objectclass_id from objectparameterimport where id = "+o.ID).First();
                                    ObjectClass oc = context.ObjektKlassen.Find(objClassID);
                                    oCopy.ObjectClass = oc;

                                    context.Objektparameter.Add(oCopy);
                                    mo.Objectparameter = oCopy;
                                    context.SaveChanges();
                                }
                            }
                        }



                        bc = new Events.MapControl_BusyStateChange();
                        bc.BusyState = Events.BusyState.Idle;
                        bc.KeyOfSender = "ImportProject";
                        bc.Percent = 100;
                        bc.Message = Resources.MapControl_Importing_Success;
                        MapControlTools.On_BusyStateChange(bc);

                        return p.Id;

                    }
                }

            }
            catch(Exception e)
            {
                Events.MapControl_Error export_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.ImportExportError, InMethod = "ImportProject", AxMapError = e.ToString() };
                On_Error(export_error);
            }
            return -1;

        }

            #endregion ImportFunctions

        }

    class ExportImportCallback : ICallback
    {

        MapControlTools MapControlTool { get; set; }
        public event EventHandler GdalFinished;

        public ExportImportCallback(MapControlTools mapControlTool) : base()
        {
            this.MapControlTool = mapControlTool;

        }
        public void Error(string KeyOfSender, string ErrorMsg)
        {

            Events.MapControl_Error export_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.ImportExportError, InMethod = "ExportProject", AxMapError = ErrorMsg };
            MapControlTool.On_Error(export_error);
        }
        public void Progress(string KeyOfSender, int Percent, string Message)
        {
            if (Percent == 100) On_GdalFinished(new EventArgs());
        }

        public virtual void On_GdalFinished(EventArgs e)
        {
            GdalFinished?.Invoke(this, e);
        }

    }
}
