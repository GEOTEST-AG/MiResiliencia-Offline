using AxMapWinGIS;
using MapWinGIS;
using ResTB.DB.Models;
using ResTB.Map.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Tools
{
    public class EditingTool : BaseTool
    {

        private ILayer _currentEditingLayer;
        /// <summary>
        /// The layer that is marked for editing
        /// </summary>
        public ILayer CurrentEditingLayer { get { return _currentEditingLayer; } /*set { _currentEditingLayer = value; }*/ }

        private bool saveAndCloseWhenFinish = false;
        private bool isAdding = false;

        public EditingTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTool) : base(axMap, mapControlTool)
        {
        }

        public bool StartEditingLayer(ResTBHazardMapLayer hazardMapLayer, bool saveAndStopWhenFinish = false)
        {
            if (!MapControlTools.Layers.Where(m => m.Name.Equals(hazardMapLayer.Name)).Any())
            {
                MapControlTools.AddProjectLayer(hazardMapLayer);
            }
            return StartEditingLayer(MapControlTools.Layers.Where(m => m.Name.Equals(hazardMapLayer.Name)).Select(m => m.Name).First(), saveAndStopWhenFinish);
        }

        public bool StartEditingLayer(DB.Models.Objectparameter objectparameter, bool saveAndStopWhenFinish = false)
        {
            ResTBDamagePotentialLayer restbDamagePotLayer = (ResTBDamagePotentialLayer)MapControlTools.Layers
                .Where(m => m.Name.Equals(MapControlTools.GetLayerNamesFromPostGISType(ResTBPostGISType.DamagePotential).First())).First();
            restbDamagePotLayer.CurrentObjectparameter = objectparameter;
            return StartEditingLayer(restbDamagePotLayer.Name, saveAndStopWhenFinish, objectparameter.FeatureType);
        }

        public bool StartEditingLayer(string name, bool saveAndStopWhenFinish = false, FeatureType featureType = FeatureType.Any)
        {
            ILayer editLayer = MapControlTools.Layers.Where(m => m.Name == name).FirstOrDefault();
            if (editLayer == null) return false;

            this.saveAndCloseWhenFinish = saveAndStopWhenFinish;

            Shapefile sf;

            var ogrLayer = AxMap.get_OgrLayer(editLayer.Handle);
            sf = AxMap.get_Shapefile(editLayer.Handle);

            _currentEditingLayer = editLayer;

            if (ogrLayer != null)
            {
                // Add the editing layer
                DBConnectionTool dBConnection = new DBConnectionTool(AxMap, MapControlTools);
                OgrDatasource ds = new OgrDatasource();
                if (!ds.Open(dBConnection.GetGdalConnectionString()))
                {
                    Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.CouldNotConnectDatabase, InMethod = "AddPostGISLayer", AxMapError = ds.GdalLastErrorMsg };
                    On_Error(error);
                    return false;
                }
                OgrLayer editlayer = null;
                if (featureType == FeatureType.Point)
                {
                    editlayer = ds.GetLayerByName(((ResTBPostGISLayer)editLayer).SQL_Layer + "(point)", true);
                    if (editLayer.GetType() == typeof(ResTBDamagePotentialLayer)) editLayer.Handle = ((ResTBDamagePotentialLayer)editLayer).PointHandle;
                }
                else if (featureType == FeatureType.Line)
                {
                    editlayer = ds.GetLayerByName(((ResTBPostGISLayer)editLayer).SQL_Layer + "(line)", true);
                    if (editLayer.GetType() == typeof(ResTBDamagePotentialLayer)) editLayer.Handle = ((ResTBDamagePotentialLayer)editLayer).LineHandle;
                }
                else if (featureType == FeatureType.Polygon)
                {
                    editlayer = ds.GetLayerByName(((ResTBPostGISLayer)editLayer).SQL_Layer + "(polygon)", true);
                    if (editLayer.GetType() == typeof(ResTBDamagePotentialLayer)) editLayer.Handle = ((ResTBDamagePotentialLayer)editLayer).PolygonHandle;
                }
                else editlayer = ds.GetLayerByName(((ResTBPostGISLayer)editLayer).SQL_Layer, true);

                int editinghandle = AxMap.AddLayer(editlayer, false);
                ((ResTBPostGISLayer)editLayer).EditingLayer = editlayer;
                ((ResTBPostGISLayer)editLayer).EditingLayerHandle = editinghandle;

                _currentEditingLayer = editLayer;

                OgrLayer ogrLayer2 = ((ResTBPostGISLayer)editLayer).EditingLayer;
                if (ogrLayer2.DynamicLoading)
                {
                    Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.EditingNotAllowed, InMethod = "StartEditingLayer", AxMapError = "" };
                    On_Error(error);
                    return false;
                }
                if (!ogrLayer2.SupportsEditing[tkOgrSaveType.ostSaveAll])
                {
                    Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.EditingNotSupported, InMethod = "StartEditingLayer", AxMapError = ogrLayer2.ErrorMsg[ogrLayer.LastErrorCode] };
                    On_Error(error);
                    return false;
                }

                AxMap.set_LayerVisible(editLayer.Handle, false);

                AxMap.set_LayerVisible(((ResTBPostGISLayer)editLayer).EditingLayerHandle, true);
                sf = AxMap.get_Shapefile(((ResTBPostGISLayer)editLayer).EditingLayerHandle);

                Utils utils = new Utils();
                if (featureType == FeatureType.Point)
                {
                    sf.DefaultDrawingOptions.PointSize = 15;
                    sf.DefaultDrawingOptions.FillColor = utils.ColorByName(tkMapColor.Blue);
                }
                else
                {
                    sf.DefaultDrawingOptions.LineWidth = 7;
                    sf.DefaultDrawingOptions.FillColor = utils.ColorByName(tkMapColor.Blue);
                    sf.DefaultDrawingOptions.LineColor = utils.ColorByName(tkMapColor.Blue);
                    AxMap.ShapeEditor.FillColor = utils.ColorByName(tkMapColor.Blue);
                }

                    //AxMap.ShapeEditor.LineWidth = 20;
                    AxMap.ShapeEditor.LineColor = utils.ColorByName(tkMapColor.Blue);
                sf.VisibilityExpression = ((ResTBPostGISLayer)editLayer).VisibilityExpression;
            }

            AxMap.SendMouseDown = true;
            AxMap.SendMouseUp = true;

            AxMap.ChooseLayer += AxMap_ChooseLayer;
            AxMap.AfterShapeEdit += _map_AfterShapeEdit;
            AxMap.BeforeDeleteShape += _map_BeforeDeleteShape;
            AxMap.BeforeShapeEdit += _map_BeforeShapeEdit;
            AxMap.ShapeValidationFailed += _map_ShapeValidationFailed;
            AxMap.ValidateShape += _map_ValidateShape;

            AxMap.ShapeEditor.IndicesVisible = false;
            AxMap.ShapeEditor.ShowLength = false;
            AxMap.ShapeEditor.ShowArea = false;
            AxMap.ShapeEditor.ValidationMode = tkEditorValidation.evFixWithGeos;

            if ((featureType == FeatureType.Point) || (featureType == FeatureType.Line)) AxMap.ShapeEditor.SnapBehavior = tkLayerSelection.lsNoLayer;
            else AxMap.ShapeEditor.SnapBehavior = tkLayerSelection.lsAllLayers;
            
            sf.InteractiveEditing = true;
            Events.MapControl_EditingStateChange editingStateChange = new Events.MapControl_EditingStateChange() { EditingState = Events.EditingState.StartEditing, EditingLayer = editLayer };
            On_EditingStateChange(editingStateChange);



            return true;
        }

        public bool StopEditingLayer(bool saveEdits = false)
        {
            if (CurrentEditingLayer == null)
            {
                // there is nothing to save...
                return false;
            }

            saveAndCloseWhenFinish = false;


            AxMap.ShapeEditor.SaveChanges();

            if (saveEdits)
            {
                if (!SaveEdits())
                {
                    //return false;
                }
            }


            var sf = AxMap.get_Shapefile(CurrentEditingLayer.Handle);
            var ogrLayer = AxMap.get_OgrLayer(CurrentEditingLayer.Handle);
            if (ogrLayer != null)
            {
                ogrLayer.ReloadFromSource();
                // show the editing layer (with all Polygons)
                sf = AxMap.get_Shapefile(((ResTBPostGISLayer)CurrentEditingLayer).EditingLayerHandle);
                AxMap.RemoveLayer(((ResTBPostGISLayer)CurrentEditingLayer).EditingLayerHandle);
                AxMap.set_LayerVisible(CurrentEditingLayer.Handle, true);

            }
            bool returnBool = sf.StopEditingShapes();

            AxMap.ChooseLayer -= AxMap_ChooseLayer;
            AxMap.AfterShapeEdit -= _map_AfterShapeEdit;
            AxMap.BeforeDeleteShape -= _map_BeforeDeleteShape;
            AxMap.BeforeShapeEdit -= _map_BeforeShapeEdit;
            AxMap.ShapeValidationFailed -= _map_ShapeValidationFailed;
            AxMap.ValidateShape -= _map_ValidateShape;

            MapControlTools.Redraw(true);
            Events.MapControl_LayerChange layerChange = new Events.MapControl_LayerChange() { Layer = this.CurrentEditingLayer, LayerChangeReason = Events.LayerChangeReason.EditedLayer };
            On_LayerChange(layerChange);
            Events.MapControl_EditingStateChange editingStateChange = new Events.MapControl_EditingStateChange() { EditingState = Events.EditingState.StopEditing, EditingLayer = CurrentEditingLayer };
            On_EditingStateChange(editingStateChange);

            _currentEditingLayer = null;

            AxMap.CursorMode = tkCursorMode.cmPan;

            return returnBool;


        }


        public bool AddShape(bool saveAndStopWhenFinish = false)
        {
            if (CurrentEditingLayer == null)
            {
                Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.UseStartEditing, InMethod = "AddLayer", AxMapError = "" };
                On_Error(error);
                return false;
            }
            this.saveAndCloseWhenFinish = saveAndStopWhenFinish;

            AxMap.CursorMode = tkCursorMode.cmAddShape;
            AxMap.MapCursor = tkCursor.crsrMapDefault;

            isAdding = true;

            Events.MapControl_EditingStateChange editingStateChange = new Events.MapControl_EditingStateChange() { EditingState = Events.EditingState.AddShape, EditingLayer = CurrentEditingLayer };
            On_EditingStateChange(editingStateChange);
            return true;
        }

        public bool EditShape(int shapeIndex = -1)
        {
            if (CurrentEditingLayer == null)
            {
                Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.UseStartEditing, InMethod = "EditingLayer", AxMapError = "" };
                On_Error(error);
                return false;
            }

            AxMap.CursorMode = tkCursorMode.cmEditShape;
            AxMap.MapCursor = tkCursor.crsrMapDefault;
            isAdding = false;

            if ((CurrentEditingLayer.GetType() == typeof(ResTBPostGISLayer)) || ((CurrentEditingLayer.GetType().BaseType != null && CurrentEditingLayer.GetType().BaseType == typeof(ResTBPostGISLayer))))
            {
                var sf = AxMap.get_Shapefile(((ResTBPostGISLayer)CurrentEditingLayer).EditingLayerHandle);

                // look for the selectedShape in the editing layer
                var sfOriginal = AxMap.get_Shapefile(((ResTBPostGISLayer)CurrentEditingLayer).Handle);
                var selectedShape = sfOriginal.Shape[shapeIndex];
                Shape selectedPointShape = null;
                Shape selectedLineShape = null;
                Shape selectedPolygonShape = null;
                int toSelectShape = -1;
                if (((ResTBPostGISLayer)CurrentEditingLayer).GetType() == typeof(ResTBDamagePotentialLayer))
                {
                    sfOriginal = AxMap.get_Shapefile(((ResTBDamagePotentialLayer)CurrentEditingLayer).PointHandle);
                    selectedPointShape = sfOriginal.Shape[shapeIndex];
                    sfOriginal = AxMap.get_Shapefile(((ResTBDamagePotentialLayer)CurrentEditingLayer).LineHandle);
                    selectedLineShape = sfOriginal.Shape[shapeIndex];
                    sfOriginal = AxMap.get_Shapefile(((ResTBDamagePotentialLayer)CurrentEditingLayer).PolygonHandle);
                    selectedPolygonShape = sfOriginal.Shape[shapeIndex];

                }


                var project_index = sf.FieldIndexByName[((ResTBPostGISLayer)CurrentEditingLayer).ProjectID];

                List<int> visibleShapes = new List<int>();
                for (int i = 0; i < sf.NumShapes; i++)
                {
                    if (sf.CellValue[project_index, i] == ((ResTBPostGISLayer)CurrentEditingLayer).Project) visibleShapes.Add(i);
                    if (sf.Shape[i].Equals(selectedShape))
                    {
                        // we could have the exact same polygone in before and after. So have a look, if before is the same
                        if (CurrentEditingLayer.GetType() == typeof(ResTBHazardMapLayer))
                        {
                            var before_index = sf.FieldIndexByName["BeforeAction"];
                            var isBefore = sf.CellValue[before_index, i];
                            if (((ResTBHazardMapLayer)CurrentEditingLayer).ResTBPostGISType == ResTBPostGISType.HazardMapBefore) {
                                if (isBefore == 1) toSelectShape = i;
                            }
                            else if (((ResTBHazardMapLayer)CurrentEditingLayer).ResTBPostGISType == ResTBPostGISType.HazardMapAfter)
                            {
                                if (isBefore == 0) toSelectShape = i;
                            }

                        }
                        else toSelectShape = i;


                    }
                    if (sf.Shape[i].Equals(selectedPointShape)) toSelectShape = i;
                    if (sf.Shape[i].Equals(selectedLineShape)) toSelectShape = i;
                    if (sf.Shape[i].Equals(selectedPolygonShape)) toSelectShape = i;
                }
                

                // if we have only one shape, mark it as active
                if (visibleShapes.Count == 1)
                {
                    AxMap.ShapeEditor.Clear();
                    bool isok = AxMap.ShapeEditor.StartEdit(((ResTBPostGISLayer)CurrentEditingLayer).EditingLayerHandle, visibleShapes[0]);
                }
                else if (shapeIndex > -1)
                {
                    AxMap.ShapeEditor.Clear();
                    bool isok = AxMap.ShapeEditor.StartEdit(((ResTBPostGISLayer)CurrentEditingLayer).EditingLayerHandle, toSelectShape);
                }
            }


            Events.MapControl_EditingStateChange editingStateChange = new Events.MapControl_EditingStateChange() { EditingState = Events.EditingState.EditSshape, EditingLayer = CurrentEditingLayer };
            On_EditingStateChange(editingStateChange);
            return true;
        }

        public bool DeleteShape()
        {
            isAdding = false;
            if (AxMap.ShapeEditor.ShapeIndex >= 0)
            {
                var sf = AxMap.get_Shapefile(((ResTBPostGISLayer)CurrentEditingLayer).EditingLayerHandle);


                // Check if DamageExtend is null, else delte it
                if (((ResTBPostGISLayer)CurrentEditingLayer).GetType() == typeof(ResTBDamagePotentialLayer))
                {
                    MappedObject mo = (MappedObject)((ResTBDamagePotentialLayer)CurrentEditingLayer).GetObjectFromShape(sf, AxMap.ShapeEditor.ShapeIndex);




                    using (DB.ResTBContext db = new DB.ResTBContext())
                    {
                        int ProjectID = ((ResTBPostGISLayer)CurrentEditingLayer).Project;
                        var damageExtents = db.DamageExtents.Where(m => m.MappedObjectId == mo.ID).ToList();
                        foreach (DamageExtent de in damageExtents)
                        {
                            db.DamageExtents.Remove(de);                            
                        }

                        var resilienceValues = db.ResilienceValues.Where(m => m.MappedObject.ID == mo.ID);
                        foreach (ResilienceValues rv in resilienceValues)
                        {
                            db.ResilienceValues.Remove(rv);
                        }

                        db.SaveChanges();
                    }

                }



                
                sf.EditDeleteShape(AxMap.ShapeEditor.ShapeIndex);
                Events.MapControl_EditingStateChange editingStateChange = new Events.MapControl_EditingStateChange() { EditingState = Events.EditingState.DeleteShape, EditingLayer = CurrentEditingLayer };
                On_EditingStateChange(editingStateChange);
            }
            return true;
        }

        public bool DeleteAllShapes()
        {
            isAdding = false;
            if (CurrentEditingLayer == null)
            {
                Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.UseStartEditing, InMethod = "StopEditingLayer", AxMapError = "" };
                On_Error(error);
                return false;
            }

            var sf = AxMap.get_Shapefile(CurrentEditingLayer.Handle);

            for (int i = 0; i < sf.NumShapes; i++)
            {
                sf.Shape[i].Clear();
            }

            return true;
        }

        public void Undo()
        {
            AxMap.Undo();
            //return AxMap.ShapeEditor.UndoPoint();
        }

        public bool SaveEdits()
        {
            if (CurrentEditingLayer == null)
            {
                Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.UseStartEditing, InMethod = "StopEditingLayer", AxMapError = "" };
                On_Error(error);
                return false;
            }
            Events.MapControl_EditingStateChange editingStateChange = new Events.MapControl_EditingStateChange() { EditingState = Events.EditingState.SaveEditing, EditingLayer = CurrentEditingLayer };
            On_EditingStateChange(editingStateChange);
            var sf = AxMap.get_Shapefile(CurrentEditingLayer.Handle);
            var ogrLayer = AxMap.get_OgrLayer(CurrentEditingLayer.Handle);

            bool success = false;
            if (ogrLayer != null)
            {
                ogrLayer = ((ResTBPostGISLayer)CurrentEditingLayer).EditingLayer;

                ((ResTBPostGISLayer)CurrentEditingLayer).SaveAttributes(AxMap);

                int savedCount;
                tkOgrSaveResult saveResult = ogrLayer.SaveChanges(out savedCount);
                success = saveResult == tkOgrSaveResult.osrAllSaved || saveResult == tkOgrSaveResult.osrNoChanges;

                // if hazard map before --> copy to hazardmap after
                if ((success) && (((ResTBPostGISLayer)CurrentEditingLayer).ResTBPostGISType == ResTBPostGISType.HazardMapBefore) && (isAdding))
                {
                    string newName = ((ResTBHazardMapLayer)CurrentEditingLayer).CopyToAfter(MapControlTools);
                    
                    ILayer HMAfter = MapControlTools.Layers.Where(m => m.Name == newName).FirstOrDefault();
                    if (HMAfter != null)
                    {
                        ogrLayer = AxMap.get_OgrLayer(HMAfter.Handle);
                        ogrLayer.ReloadFromSource();
                    }
                }


                ogrLayer = AxMap.get_OgrLayer(CurrentEditingLayer.Handle);
                ogrLayer.ReloadFromSource();


            }

            if (success)
            {
                sf.InteractiveEditing = false;
                AxMap.ShapeEditor.Clear();
                AxMap.UndoList.ClearForLayer(CurrentEditingLayer.Handle);
                Events.MapControl_EditingStateChange editingStateChangeStop = new Events.MapControl_EditingStateChange() { EditingState = Events.EditingState.StopEditing, EditingLayer = CurrentEditingLayer };
                On_EditingStateChange(editingStateChangeStop);

                Events.MapControl_LayerChange layerChange = new Events.MapControl_LayerChange() { Layer = this.CurrentEditingLayer, LayerChangeReason = Events.LayerChangeReason.EditedLayer };
                On_LayerChange(layerChange);
                return true;
            }
            AxMap.Redraw();
            return false;
        }

        private void _map_ValidateShape(object sender, _DMapEvents_ValidateShapeEvent e)
        {

        }

        private void _map_ShapeValidationFailed(object sender, _DMapEvents_ShapeValidationFailedEvent e)
        {
           

            Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.ShapeInvalid, InMethod = "_map_ShapeValidationFailed", AxMapError = e.errorMessage };
            On_Error(error);
        }

        private void _map_BeforeShapeEdit(object sender, _DMapEvents_BeforeShapeEditEvent e)
        {

        }

        private void _map_BeforeDeleteShape(object sender, _DMapEvents_BeforeDeleteShapeEvent e)
        {
        }

        private void _map_AfterShapeEdit(object sender, _DMapEvents_AfterShapeEditEvent e)
        {
            if (saveAndCloseWhenFinish)
            {
                StopEditingLayer(saveAndCloseWhenFinish);
            }
            else
            {
                ((ResTBPostGISLayer)CurrentEditingLayer).SaveAttributes(AxMap);
            }
        }

        private void AxMap_ChooseLayer(object sender, AxMapWinGIS._DMapEvents_ChooseLayerEvent e)
        {
            Type editingLayerType = CurrentEditingLayer.GetType();
            if (editingLayerType.BaseType != null && editingLayerType.BaseType == typeof(ResTBPostGISLayer))
            {
                e.layerHandle = ((ResTBPostGISLayer)CurrentEditingLayer).EditingLayerHandle;
            }
            else if (CurrentEditingLayer.GetType() == typeof(ResTBPostGISLayer))
            {
                e.layerHandle = ((ResTBPostGISLayer)CurrentEditingLayer).EditingLayerHandle;
            }

            else
            {
                e.layerHandle = CurrentEditingLayer.Handle;
            }
        }
    }
}
