using MapWinGIS;
using ResTB.DB.Models;
using ResTB.Map.Layer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Threading;
using ResTB.Translation.Properties;

namespace ResTB.Map.Tools
{
    public class AddLayersTool : BaseTool
    {

        private Thread reprojectThread;

        public DBConnectionTool dBConnection;
        private OgrDatasource ds;

        public AddLayersTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTool) : base(axMap, mapControlTool)
        {
            dBConnection = new DBConnectionTool(AxMap, MapControlTools);
        }

        public bool AddWMSLayer(string baseUrl, string layers, string name, Extents extents, int Epsg, string Format)
        {
            WmsLayer wmsLayer = new WmsLayer();

            wmsLayer.BaseUrl = baseUrl;
            wmsLayer.BoundingBox = extents;
            wmsLayer.DoCaching = false;
            wmsLayer.Epsg = Epsg;
            wmsLayer.Format = Format;
            wmsLayer.Layers = layers;
            wmsLayer.Name = name;
            wmsLayer.UseCache = false;
            wmsLayer.Id = 1;
            wmsLayer.Key = "1";
            wmsLayer.Version = tkWmsVersion.wv111;

            WmsLayerLayer layer = new WmsLayerLayer();
            layer.WmsLayerObj = wmsLayer;
            layer.Name = name;
            layer.Handle = AxMap.AddLayer(wmsLayer, true);
            layer.LayerType = LayerType.CustomLayerWMS;

            MapControlTools.Layers.Add(layer);
            Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.AddLayer, Layer = layer };
            On_LayerChange(layerchange);

            return true;
        }

        public bool AddSHPLayer()
        {
            string filename = @"c:\users\suter\documents\test.shp";
            Shapefile sf = new Shapefile();
            if (sf.Open(filename))
            {
                int m_layerHandle = AxMap.AddLayer(sf, true);
                sf = AxMap.get_Shapefile(m_layerHandle);     // in case a copy of shapefile was created by GlobalSettings.ReprojectLayersOnAdding

                ShpLayer sl = new ShpLayer();
                sl.LayerType = LayerType.CustomLayerSHP;
                sl.Name = "Gefahrenkarte";
                sl.Shapefile = sf;
                sl.Handle = m_layerHandle;

                MapControlTools.Layers.Add(sl);
                Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.AddLayer, Layer = sl };
                On_LayerChange(layerchange);
                return true;
            }
            else
            {
                Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotLoadLayer, InMethod = "AddShpLayer", AxMapError = sf.ErrorMsg[sf.LastErrorCode] };
                On_Error(error);
                return false;
            }
        }

        public bool IsRasterGoogleMercator(string fileLocation)
        {
            Image img = new Image();
            if (!img.Open(fileLocation))
            {
                Events.MapControl_Error errorFileNotOpen = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotLoadLayer, InMethod = "AddRasterLayer", AxMapError = img.ErrorMsg[img.LastErrorCode] };
                On_Error(errorFileNotOpen);
            }
            return IsRasterGoogleMercator(img);
        }

        public bool IsRasterGoogleMercator(Image img)
        {
            string proj = img.GetProjection();
            if (img.GetProjection() == "+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext +no_defs") return true;
            else return false;
        }

        public bool AddRasterLayer(string fileLocation, string layerName, bool autoReproject = true, bool overwriteExistingReprojectedFiles = false)
        {
            Image img = new Image();

            if (autoReproject)
            {
                if (!img.Open(fileLocation))
                {
                    Events.MapControl_Error errorFileNotOpen = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotLoadLayer, InMethod = "AddRasterLayer", AxMapError = img.ErrorMsg[img.LastErrorCode] };
                    On_Error(errorFileNotOpen);
                }

                if (!IsRasterGoogleMercator(img))
                {
                    // Image is not WGS84, so use GdalWrap for reprojection

                    string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if (!System.IO.Directory.Exists(localData + "\\ResTBDesktop")) System.IO.Directory.CreateDirectory(localData + "\\ResTBDesktop");

                    string filename = Path.GetFileName(fileLocation);
                    if (!Directory.Exists(localData + "\\ResTBDesktop\\temp")) Directory.CreateDirectory(localData + "\\ResTBDesktop\\temp");

                    // check if file already transformed
                    if ((File.Exists(localData + "\\ResTBDesktop\\temp\\" + filename)) && (!overwriteExistingReprojectedFiles))
                    {

                    }
                    else
                    {
                        string fileLocationThreadSave = (string)fileLocation.Clone();
                        fileLocation = localData + "\\ResTBDesktop\\temp\\" + filename;
                        RasterLayer r = new RasterLayer() { FileName = fileLocation, LayerType = LayerType.CustomLayerRaster, Name = layerName };

                        RasterProjectionWorker rpw = new RasterProjectionWorker();
                        rpw.MapControlTools = MapControlTools;
                        rpw.MapControl_RasterReprojected += Callback_MapControl_RasterReprojected;

                        reprojectThread = new Thread(() => rpw.Run(filename, fileLocationThreadSave, r));

                        Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
                        bc.BusyState = Events.BusyState.BackgroundBusy;
                        bc.KeyOfSender = Resources.MapControl_Busy_RasterReprojection;
                        bc.Percent = 0;
                        bc.Message = Resources.MapControl_Busy_StartRasterReprojection; ;

                        MapControlTools.On_BusyStateChange(bc);

                        reprojectThread.Start();

                        return false;

                    }
                    fileLocation = localData + "\\ResTBDesktop\\temp\\" + filename;
                }

            }

            if (img.Open(fileLocation))
            {
                string proj = img.GetProjection();
                int layerHandle = AxMap.AddLayer(img, true);
                if (layerHandle == -1)
                {
                    Events.MapControl_Error imghandle_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotLoadLayer, InMethod = "AddRasterLayer", AxMapError = new GlobalSettings().GdalLastErrorMsg };
                    On_Error(imghandle_error);
                    return false;
                }

                RasterLayer r = new RasterLayer() { FileName = fileLocation, Handle = layerHandle, LayerType = LayerType.CustomLayerRaster, Name = layerName };
                MapControlTools.Layers.Add(r);
                MapControlTools.LayerHandlingTool.SetLayerPosition(r.Name, LayerMoveType.BOTTOM);
                Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.AddLayer, Layer = r };
                On_LayerChange(layerchange);
                return true;
            }
            Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotLoadLayer, InMethod = "AddRasterLayer", AxMapError = img.ErrorMsg[img.LastErrorCode] };
            On_Error(error);
            return false;
        }

        private bool AddProjectLayer(ResTBDamagePotentialLayer resTBDamagePotentialLayer, bool visible = true)
        {
            if (!MapControlTools.Layers.Where(m => m.Name == resTBDamagePotentialLayer.Name).Any())
            {


                var pointlayer = ds.RunQuery(resTBDamagePotentialLayer.SQL_Point);
                var linelayer = ds.RunQuery(resTBDamagePotentialLayer.SQL_Line);
                var polygonlayer = ds.RunQuery(resTBDamagePotentialLayer.SQL_Polygon);
                if ((pointlayer == null) || (linelayer == null) || (polygonlayer == null))
                {
                    Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.FailedToRunSQLQuery, InMethod = "AddPostGISLayer", AxMapError = ds.GdalLastErrorMsg };
                    On_Error(error);
                    return false;
                }
                else
                {
                        int pointhandle = AxMap.AddLayer(pointlayer, visible);
                    int linehandle = AxMap.AddLayer(linelayer, visible);
                    int polygonhandle = AxMap.AddLayer(polygonlayer, visible);

                    if ((pointhandle == -1) || (linehandle == -1) || (polygonhandle == -1))
                    {
                        Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotLoadLayer, InMethod = "AddPostGISLayer", AxMapError = AxMap.FileManager.get_ErrorMsg(AxMap.FileManager.LastErrorCode) };
                        On_Error(error);
                        return false;
                    }
                    else
                    {

                        resTBDamagePotentialLayer.PointHandle = pointhandle;
                        resTBDamagePotentialLayer.LineHandle = linehandle;
                        resTBDamagePotentialLayer.PolygonHandle = polygonhandle;
                        resTBDamagePotentialLayer.ApplyStyle(AxMap);

                        MapControlTools.Layers.Add(resTBDamagePotentialLayer);
                        Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.AddLayer, Layer = resTBDamagePotentialLayer };
                        On_LayerChange(layerchange);

                    }

                    return true;
                }



            }
            return false;
        }

        private bool AddProjectLayer(ResTBRiskMapLayer resTBRiskMapLayer, bool visible = true)
        {
            if (!MapControlTools.Layers.Where(m => m.Name == resTBRiskMapLayer.Name).Any())
            {
                var pointlayer = ds.RunQuery(resTBRiskMapLayer.SQL_Point);
                var linelayer = ds.RunQuery(resTBRiskMapLayer.SQL_Line);
                var polygonlayer = ds.RunQuery(resTBRiskMapLayer.SQL_Polygon);
                if ((pointlayer == null) || (linelayer == null) || (polygonlayer == null))
                {
                    Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.FailedToRunSQLQuery, InMethod = "AddPostGISLayer", AxMapError = ds.GdalLastErrorMsg };
                    On_Error(error);
                    return false;
                }
                else
                {
                    int pointhandle = AxMap.AddLayer(pointlayer, visible);
                    int linehandle = AxMap.AddLayer(linelayer, visible);
                    int polygonhandle = AxMap.AddLayer(polygonlayer, visible);

                    if ((pointhandle == -1) || (linehandle == -1) || (polygonhandle == -1))
                    {
                        Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotLoadLayer, InMethod = "AddPostGISLayer", AxMapError = AxMap.FileManager.get_ErrorMsg(AxMap.FileManager.LastErrorCode) };
                        On_Error(error);
                        return false;
                    }
                    else
                    {

                        resTBRiskMapLayer.PointHandle = pointhandle;
                        resTBRiskMapLayer.LineHandle = linehandle;
                        resTBRiskMapLayer.PolygonHandle = polygonhandle;
                        resTBRiskMapLayer.ApplyStyle(AxMap);

                        MapControlTools.Layers.Add(resTBRiskMapLayer);
                        Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.AddLayer, Layer = resTBRiskMapLayer };
                        On_LayerChange(layerchange);

                    }

                    return true;
                }



            }
            return false;
        }

        public bool AddProjectLayer(ResTBPostGISLayer resTBPostGISLayer, bool visible = true)
        {
            if (!MapControlTools.Layers.Where(m => m.Name == resTBPostGISLayer.Name).Any())
            {
                Type t = resTBPostGISLayer.GetType();

                // Handle the damage potential differently
                if (resTBPostGISLayer.GetType() == typeof(ResTBDamagePotentialLayer)) return AddProjectLayer((ResTBDamagePotentialLayer)resTBPostGISLayer, visible);
                if (resTBPostGISLayer.GetType() == typeof(ResTBRiskMapLayer)) return AddProjectLayer((ResTBRiskMapLayer)resTBPostGISLayer, visible);

                var layer = ds.RunQuery(resTBPostGISLayer.SQL);
                if (layer == null)
                {
                    Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.FailedToRunSQLQuery, InMethod = "AddPostGISLayer", AxMapError = ds.GdalLastErrorMsg };
                    On_Error(error);
                    return false;
                }
                else
                {
                    int handle = AxMap.AddLayer(layer, visible);

                    if (handle == -1)
                    {
                        Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotLoadLayer, InMethod = "AddPostGISLayer", AxMapError = AxMap.FileManager.get_ErrorMsg(AxMap.FileManager.LastErrorCode) };
                        On_Error(error);


                        return false;
                    }
                    else
                    {
                        resTBPostGISLayer.Handle = handle;

                        resTBPostGISLayer.ApplyStyle(AxMap);

                        MapControlTools.Layers.Add(resTBPostGISLayer);
                        Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.AddLayer, Layer = resTBPostGISLayer };
                        On_LayerChange(layerchange);

                    }

                    return true;
                }



            }
            return false;
        }


        public bool AddProjectLayer(int Project, ResTBPostGISType resTBPostGISType)
        {
            ResTBPostGISLayer resTBPostGISLayer = new ResTBPostGISLayer();
            bool visibleAtStartup = false;
            switch (resTBPostGISType)
            {
                case ResTBPostGISType.Perimeter:
                    resTBPostGISLayer = new ResTBPerimeterLayer(Project);
                    visibleAtStartup = true;
                    break;
                case ResTBPostGISType.MitigationMeasure:
                    resTBPostGISLayer = new ResTBMitigationMeasureLayer(Project);
                    break;
                case ResTBPostGISType.DamagePotential:
                    resTBPostGISLayer = new ResTBDamagePotentialLayer(Project);
                    break;
                case ResTBPostGISType.ResilienceBefore:
                    resTBPostGISLayer = new ResTBResilienceLayer(Project, true);
                    break;
                case ResTBPostGISType.ResilienceAfter:
                    resTBPostGISLayer = new ResTBResilienceLayer(Project, false);
                    break;
                case ResTBPostGISType.RiskMap:
                    resTBPostGISLayer = new ResTBRiskMapLayer(Project);
                    break;
                case ResTBPostGISType.RiskMapAfter:
                    resTBPostGISLayer = new ResTBRiskMapLayer(Project, false);
                    break;
            }

            return AddProjectLayer(resTBPostGISLayer, visibleAtStartup);
        }

        /// <summary>
        /// Add all Layers for a project. (Hazard Map only the ones with data)
        /// </summary>
        /// <param name="Project">The project id</param>
        /// <returns></returns>
        public bool AddProjectLayers(int Project)
        {
            Thread addProjectThread = new Thread(() => AddProjectLayersThread(Project));

            Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
            bc.BusyState = Events.BusyState.BackgroundBusy;
            bc.KeyOfSender = "AddProjectLayers";
            bc.Percent = 0;
            bc.Message = Resources.MapControl_AddProjectLayers;

            MapControlTools.On_BusyStateChange(bc);

            addProjectThread.Start();

            return true;
        }

        public void AddProjectLayersThread(int Project)
        {
            var gdalUtils = new GdalUtils();

            RasterReprojectCallback callback = new RasterReprojectCallback(null, MapControlTools);
            gdalUtils.GlobalCallback = callback;
            ds = new OgrDatasource();
            if (!ds.Open(dBConnection.GetGdalConnectionString()))
            {
                Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotConnectDatabase, InMethod = "AddPostGISLayer", AxMapError = ds.GdalLastErrorMsg };
                On_Error(error);
                return;
            }
            else
            {
                Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "AddProjectLayersThread";
                bc.Percent = 10;
                bc.Message = Resources.MapControl_AddedPerimeter;
                MapControlTools.On_BusyStateChange(bc);

               
                // RiskMap
                if (!AddProjectLayer(Project, ResTBPostGISType.RiskMap)) return;
                if (!AddProjectLayer(Project, ResTBPostGISType.RiskMapAfter)) return;

                // Perimeter
                if (!AddProjectLayer(Project, ResTBPostGISType.Perimeter)) return;
                if (MapControlTools.ShapesCount(MapControlTools.GetLayerNamesFromPostGISType(ResTBPostGISType.Perimeter).First()) > 0)
                    MapControlTools.ZoomToLayer(MapControlTools.GetLayerNamesFromPostGISType(ResTBPostGISType.Perimeter).First());
                

                // Hazard Maps
                DB.ResTBContext db = new DB.ResTBContext();
                List<HazardMap> hazardMaps = db.HazardMaps.Where(m => m.Project.Id == Project).Include(m => m.NatHazard).OrderBy(m => m.Index).ToList();

                double percentadd = 50.0 / hazardMaps.Count;
                int currentpercent = 10;

                foreach (HazardMap hazardMap in hazardMaps.OrderByDescending(m=>m.BeforeAction))
                {
                    ResTBHazardMapLayer hazardLayer;
                    if (hazardMap.BeforeAction) hazardLayer = new ResTBHazardMapLayer(Project, true, hazardMap.NatHazard, hazardMap.Index);
                    else hazardLayer = new ResTBHazardMapLayer(Project, false, hazardMap.NatHazard, hazardMap.Index);


                    currentpercent += (int)percentadd;
                    if (!MapControlTools.Layers.Where(m => m.Name == hazardLayer.Name).Any())
                    {
                        AddProjectLayer(hazardLayer, false);
                        bc = new Events.MapControl_BusyStateChange();
                        bc.BusyState = Events.BusyState.Busy;
                        bc.KeyOfSender = "AddProjectLayersThread";
                        bc.Percent = currentpercent;
                        bc.Message = Resources.MapControl_AddingHazardMaps;
                        MapControlTools.On_BusyStateChange(bc);

                    }
                }


                // Damage Potential
                if (!AddProjectLayer(Project, ResTBPostGISType.DamagePotential)) return;

                // Resiliences

                if (!AddProjectLayer(Project, ResTBPostGISType.ResilienceBefore)) return;

                if (!AddProjectLayer(Project, ResTBPostGISType.ResilienceAfter)) return;

                currentpercent += 30;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Busy;
                bc.KeyOfSender = "AddProjectLayersThread";
                bc.Percent = currentpercent;
                bc.Message = Resources.MapControl_AddedDamagePotentials;
                MapControlTools.On_BusyStateChange(bc);

                // Mitigation Measure
                if (!AddProjectLayer(Project, ResTBPostGISType.MitigationMeasure)) return;

                currentpercent += 10;
                bc = new Events.MapControl_BusyStateChange();
                bc.BusyState = Events.BusyState.Idle;
                bc.KeyOfSender = "Project";
                bc.Percent = 100;
                bc.Message = Resources.MapControl_ProjectLoaded;// "Project loaded";
                MapControlTools.On_BusyStateChange(bc);


                return;
            }
        }

        public bool Redraw(bool reloadLayers=false)
        {
            Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
            bc = new Events.MapControl_BusyStateChange();
            bc.BusyState = Events.BusyState.Busy;
            bc.KeyOfSender = "Redraw";
            bc.Percent = 0;
            bc.Message = Resources.MapControl_RedrawMap;
            MapControlTools.On_BusyStateChange(bc);

            if (reloadLayers)
            {
                double percentadd = 80.0 / MapControlTools.Layers.Count;
                int currentpercent = 10;

                foreach (ILayer layer in MapControlTools.Layers)
                {
                    Type editingLayerType = layer.GetType();
                    if (editingLayerType == typeof(ResTBDamagePotentialLayer))
                    {
                        OgrLayer o = AxMap.get_OgrLayer(((ResTBDamagePotentialLayer)layer).PointHandle);
                        o.ReloadFromSource();
                        o = AxMap.get_OgrLayer(((ResTBDamagePotentialLayer)layer).LineHandle);
                        o.ReloadFromSource();
                        o = AxMap.get_OgrLayer(((ResTBDamagePotentialLayer)layer).PolygonHandle);
                        o.ReloadFromSource();
                        
                    }
                    else if (editingLayerType == typeof(ResTBRiskMapLayer))
                    {
                        OgrLayer o = AxMap.get_OgrLayer(((ResTBRiskMapLayer)layer).PointHandle);
                        o.ReloadFromSource();
                        o = AxMap.get_OgrLayer(((ResTBRiskMapLayer)layer).LineHandle);
                        o.ReloadFromSource();
                        o = AxMap.get_OgrLayer(((ResTBRiskMapLayer)layer).PolygonHandle);
                        o.ReloadFromSource();

                    }
                    else if (editingLayerType.BaseType != null && editingLayerType.BaseType == typeof(ResTBPostGISLayer))
                    {
                        OgrLayer o = AxMap.get_OgrLayer(((ResTBPostGISLayer)layer).Handle);
                        o.ReloadFromSource();
                    }

                    layer.ShapeCount = MapControlTools.ShapesCount(layer.Name);

                    currentpercent += (int)percentadd;
                    bc = new Events.MapControl_BusyStateChange();
                    bc.BusyState = Events.BusyState.Busy;
                    bc.KeyOfSender = "Redraw";
                    bc.Percent = currentpercent;
                    bc.Message = Resources.MapControl_ReloadLayers;

                }
            }

            bc = new Events.MapControl_BusyStateChange();
            bc.BusyState = Events.BusyState.Busy;
            bc.KeyOfSender = "Redraw";
            bc.Percent = 80;
            bc.Message = Resources.MapControl_RedrawMap;
            MapControlTools.On_BusyStateChange(bc);
            AxMap.Redraw();

            foreach (BaseLayer l in MapControlTools.Layers)
            {
                l.ApplyStyle(AxMap);
            }

            bc = new Events.MapControl_BusyStateChange();
            bc = new Events.MapControl_BusyStateChange();
            bc.BusyState = Events.BusyState.Idle;
            bc.KeyOfSender = "Redraw";
            bc.Percent = 100;
            bc.Message = Resources.MapControl_MapRefreshed;
            MapControlTools.On_BusyStateChange(bc);

            //MapControlTools.ReStartSelecting();

            return true;
        }
        private void Callback_MapControl_RasterReprojected(object sender, Events.MapControl_RasterReprojected e)
        {
            Console.WriteLine("Finished");
            Image img = new Image();
            if (img.Open(e.rasterLayer.FileName))
            {
                string proj = img.GetProjection();
                int layerHandle = AxMap.AddLayer(img, true);
                if (layerHandle == -1)
                {
                    Events.MapControl_Error imghandle_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotLoadLayer, InMethod = "AddRasterLayer", AxMapError = new GlobalSettings().GdalLastErrorMsg };
                    On_Error(imghandle_error);
                }
                e.rasterLayer.Handle = layerHandle;
                MapControlTools.Layers.Add(e.rasterLayer);
                Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.AddLayer, Layer = e.rasterLayer };
                On_LayerChange(layerchange);


            }
        }

    }

    public class RasterProjectionWorker
    {
        /// <summary>
        /// Finished raster reprojection
        /// </summary>        
        public event EventHandler<Events.MapControl_RasterReprojected> MapControl_RasterReprojected;
        public MapControlTools MapControlTools { get; set; }
        public void Run(string filename, string fileLocation, RasterLayer r)
        {
            string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var output = localData + "\\ResTBDesktop\\temp\\" + filename;
            var options = new[]
            {
                            "-t_srs", "EPSG:3857",
                            "-overwrite"
                        };



            var gdalUtils = new GdalUtils();

            RasterReprojectCallback callback = new RasterReprojectCallback(r, MapControlTools);
            gdalUtils.GlobalCallback = callback;

            if (!gdalUtils.GdalRasterWarp(fileLocation, output, options))
            {
                Events.MapControl_Error imghandle_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.GdalWarpError, InMethod = "AddRasterLayer", AxMapError = "GdalWarp failed: " + gdalUtils.ErrorMsg[gdalUtils.LastErrorCode] + " Detailed error: " + gdalUtils.DetailedErrorMsg };
                MapControlTools.On_Error(imghandle_error);
            }

            if (MapControl_RasterReprojected != null)
            {
                Events.MapControl_RasterReprojected eventArgs = new Events.MapControl_RasterReprojected();
                eventArgs.rasterLayer = r;
                MapControl_RasterReprojected(this, eventArgs);
            }

            Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
            bc.BusyState = Events.BusyState.Idle;
            bc.KeyOfSender = "Rasterreprojected";
            bc.Percent = 100;
            bc.Message = "";

            MapControlTools.On_BusyStateChange(bc);

        }
    }


    class RasterReprojectCallback : ICallback
    {
        public RasterLayer r { get; set; }

        MapControlTools MapControlTool { get; set; }


        public RasterReprojectCallback(RasterLayer r, MapControlTools mapControlTool) : base()
        {
            this.r = r;
            this.MapControlTool = mapControlTool;

        }
        public void Error(string KeyOfSender, string ErrorMsg)
        {
            Console.WriteLine("Error: " + ErrorMsg);
        }
        public void Progress(string KeyOfSender, int Percent, string Message)
        {
            Events.MapControl_BusyStateChange bc = new Events.MapControl_BusyStateChange();
            bc.BusyState = Events.BusyState.BackgroundBusy;
            bc.KeyOfSender = KeyOfSender;
            bc.Percent = Percent;
            bc.Message = Message;

            MapControlTool.On_BusyStateChange(bc);

        }


    }
}
