using AxMapWinGIS;
using MapWinGIS;
using ResTB.DB.Models;
using ResTB.Map.Layer;
using ResTB.Map.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ResTB.Map
{
    public class MapControlTools
    {
        /// <summary>
        /// The refernece to the map
        /// </summary>
        public AxMapWinGIS.AxMap AxMap { get; set; }
        public List<ILayer> Layers { get; }


        /// Tools
        public AddLayersTool AddLayersTool { get; set; }
        public EditingTool EditingTool { get; set; }
        public LayerHandlingTool LayerHandlingTool { get; set; }
        public SelectObjectTool SelectObjectTool { get; set; }
        public ExportImportTool ExportImportTool { get; set; }
        public PrintTool PrintTool { get; set; }


        /// <summary>
        /// Error Event
        /// </summary>        
        public event EventHandler<Events.MapControl_Error> MapControl_Error;
        /// <summary>
        /// Layers changed Event
        /// </summary>        
        public event EventHandler<Events.MapControl_LayerChange> MapControl_LayerChange;
        /// <summary>
        /// Editing changed Event
        /// </summary>        
        public event EventHandler<Events.MapControl_EditingStateChange> MapControl_EditingStateChange;
        /// <summary>
        /// Selecting changed Event
        /// </summary>        
        public event EventHandler<Events.MapControl_SelectingStateChange> MapControl_SelectingStateChange;
        /// <summary>
        /// Shape Clicked Event
        /// </summary>        
        public event EventHandler<Events.MapControl_Clicked> MapControl_Clicked;
        /// <summary>
        /// Busy state change with percent 
        /// </summary>        
        public event EventHandler<Events.MapControl_BusyStateChange> MapControl_BusyStateChange;

        /// <summary>
        /// The functions for working with the Mapcontrol
        /// </summary>
        /// <param name="axMap"></param>
        public MapControlTools(AxMapWinGIS.AxMap axMap)
        {
            AxMap = axMap;
            Layers = new List<ILayer>();
            InitTools();

        }

        private void InitTools()
        {
            AddLayersTool = new AddLayersTool(AxMap, this);
            EditingTool = new EditingTool(AxMap, this);
            LayerHandlingTool = new LayerHandlingTool(AxMap, this);
            SelectObjectTool = new SelectObjectTool(AxMap, this);
            ExportImportTool = new ExportImportTool(AxMap, this);
            PrintTool = new PrintTool(AxMap, this);
        }


        /// AddLayersTools Methods
        public bool AddWMSLayer(string baseUrl, string layers, string name, Extents extents, int Epsg, string Format) { return AddLayersTool.AddWMSLayer(baseUrl, layers, name, extents, Epsg, Format); }
        public bool AddSHPLayer() { return AddLayersTool.AddSHPLayer(); }
        public bool IsRasterGoogleMercator(string fileLocation) { return AddLayersTool.IsRasterGoogleMercator(fileLocation); }
        public bool AddRasterLayer(string fileLocation, string layerName, bool autoReproject = true, bool overwriteExistingReprojectedFiles = false) { return AddLayersTool.AddRasterLayer(fileLocation, layerName, autoReproject, overwriteExistingReprojectedFiles); }
        public bool AddPostGISLayer(int Project, ResTBPostGISType resTBPostGISType) { return AddLayersTool.AddProjectLayer(Project, resTBPostGISType); }
        public bool AddProjectLayer(int Project, ResTBPostGISType resTBPostGISType) { return AddLayersTool.AddProjectLayer(Project, resTBPostGISType); }
        public bool AddProjectLayer(ResTBPostGISLayer resTBPostGISLayer) { return AddLayersTool.AddProjectLayer(resTBPostGISLayer); }
        public bool AddProjectLayers(int Project) { return AddLayersTool.AddProjectLayers(Project); }
        public bool Redraw(bool reloadLayers=false) { return AddLayersTool.Redraw(reloadLayers); }

        /// Editing Tools
        public bool StartEditingLayer(ResTBHazardMapLayer hazardMapLayer, bool saveAndStopWhenFinish = false) { return EditingTool.StartEditingLayer(hazardMapLayer, saveAndStopWhenFinish); }
        public bool StartEditingLayer(DB.Models.Objectparameter objectparameter, bool saveAndStopWhenFinish = false) { return EditingTool.StartEditingLayer(objectparameter, saveAndStopWhenFinish); }
        public bool StartEditingLayer(string name, bool saveAndStopWhenFinish = false, FeatureType featureType = FeatureType.Any) { return EditingTool.StartEditingLayer(name, saveAndStopWhenFinish, featureType); }
        public bool StopEditingLayer(bool saveEdits = false) { return EditingTool.StopEditingLayer(saveEdits); }
        public bool AddShape(bool saveAndStopWhenFinish = false) { return EditingTool.AddShape(saveAndStopWhenFinish); }
        public bool EditShape(int shapeIndex = -1) { return EditingTool.EditShape(shapeIndex); }
        public bool DeleteShape() { return EditingTool.DeleteShape(); }
        public bool DeleteAllShapes() { return EditingTool.DeleteAllShapes(); }
        public bool SaveEdits() { return EditingTool.SaveEdits(); }
        public void Undo() { EditingTool.Undo(); }


        // General layer handling methods
        public List<string> GetLayerNamesFromPostGISType(ResTBPostGISType type) { return LayerHandlingTool.GetLayerNamesFromPostGISType(type); }
        public bool RemoveLayer(string name) { return LayerHandlingTool.RemoveLayer(name); }
        public bool RemoveAllLayers() { return LayerHandlingTool.RemoveAllLayers(); }
        public bool ZoomToLayer(string name) { return LayerHandlingTool.ZoomToLayer(name); }
        public int ShapesCount(string name) { return LayerHandlingTool.ShapesCount(name); }
        public bool SetLayerPosition(string name, LayerMoveType moveType) { return LayerHandlingTool.SetLayerPosition(name, moveType); }
        public bool SetLayerVisible(string name, bool visible) { return LayerHandlingTool.SetLayerVisible(name, visible); }

        // SelectObjectTools
        public bool StartSelecting(string layerName) { return SelectObjectTool.StartSelecting(layerName);  }
        public bool StartSelecting(Layer.ResTBPostGISType type) { return SelectObjectTool.StartSelecting(type); }
        public bool StartSelecting(DB.Models.NatHazard natHazard, bool beforeMeasure) { return SelectObjectTool.StartSelecting(natHazard, beforeMeasure); }
        public void StopSelecting() { SelectObjectTool.StopSelecting(); }
        public void ReStartSelecting() { SelectObjectTool.ReStartSelecting(); }

        // ExportImportTool
        public bool ExportProject(int project, string filename) { return ExportImportTool.ExportProject(project, filename); }
        public int ImportProject(string filename) { return ExportImportTool.ImportProject(filename); }

        // PrintTool
        public void SaveMapAsJPEG(string filename) { PrintTool.SaveMapAsJPEG(filename); }
        public void PrintAsPDF(string filename, Project project) { PrintTool.PrintAsPDF(filename, project); }

        /// <summary>
        /// On_Error called when an Error occurred
        /// </summary>
        public virtual void On_Error(Events.MapControl_Error e) //protected virtual method
        {
            MapControl_Error?.Invoke(this, e);
        }

        /// <summary>
        /// On_LayerChange when a Layer is added, removed oder changed
        /// </summary>
        public virtual void On_LayerChange(Events.MapControl_LayerChange e) //protected virtual method
        {
            //TODO: check!
            var layer = e.Layer;
            if (layer != null)
            {
                if (layer.GetType() == typeof(ResTBDamagePotentialLayer))
                {
                    layer.LayerPosition = AxMap.get_LayerPosition(((ResTBDamagePotentialLayer)layer).PointHandle);
                    layer.IsVisible = AxMap.get_LayerVisible(((ResTBDamagePotentialLayer)layer).PointHandle);
                }
                else if (layer.GetType() == typeof(ResTBRiskMapLayer))
                {
                    layer.LayerPosition = AxMap.get_LayerPosition(((ResTBRiskMapLayer)layer).PointHandle);
                    layer.IsVisible = AxMap.get_LayerVisible(((ResTBRiskMapLayer)layer).PointHandle);
                }
                else
                {
                    layer.LayerPosition = AxMap.get_LayerPosition(layer.Handle);
                    layer.IsVisible = AxMap.get_LayerVisible(layer.Handle);
                }
                layer.ShapeCount = ShapesCount(layer.Name);
            }
            
            MapControl_LayerChange?.Invoke(this, e);
        }

        /// <summary>
        /// On_EditingStateChange when a starting editing, stopping, ...
        /// </summary>
        public virtual void On_EditingStateChange(Events.MapControl_EditingStateChange e) //protected virtual method
        {
            //TODO: check!
            var layer = e.EditingLayer;
            if (layer != null)
            {
                if (layer.GetType() == typeof(ResTBDamagePotentialLayer))
                {
                    layer.LayerPosition = AxMap.get_LayerPosition(((ResTBDamagePotentialLayer)layer).PointHandle);
                    layer.IsVisible = AxMap.get_LayerVisible(((ResTBDamagePotentialLayer)layer).PointHandle);
                }
                else
                {
                    layer.LayerPosition = AxMap.get_LayerPosition(layer.Handle);
                    layer.IsVisible = AxMap.get_LayerVisible(layer.Handle);
                }
                layer.ShapeCount = ShapesCount(layer.Name);
            }

            MapControl_EditingStateChange?.Invoke(this, e);
        }

        /// <summary>
        /// On_SelectingStateChange when a starting selecting, stopping, ...
        /// </summary>
        public virtual void On_SelectingStateChange(Events.MapControl_SelectingStateChange e) //protected virtual method
        {
            

            MapControl_SelectingStateChange?.Invoke(this, e);
        }

        /// <summary>
        /// When a Shape clicked by the Identify Event
        /// </summary>
        public virtual void On_ShapeClicked(Events.MapControl_Clicked e) //protected virtual method
        {
            MapControl_Clicked?.Invoke(this, e);
        }

        /// <summary>
        /// When a calculation or reprojection is working in background
        /// </summary>
        public virtual void On_BusyStateChange(Events.MapControl_BusyStateChange e) //protected virtual method
        {
            MapControl_BusyStateChange?.Invoke(this, e);
        }



    }
}
