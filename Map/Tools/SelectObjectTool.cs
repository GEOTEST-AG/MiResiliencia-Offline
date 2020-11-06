using MapWinGIS;
using ResTB.Map.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Tools
{
    public class SelectObjectTool : BaseTool
    {
        private List<ILayer> _currentSelectionLayers;
        public List<ILayer> CurrentSelectionLayers { get { return _currentSelectionLayers; } private set { _currentSelectionLayers = value; } }

        private bool _isEditing;
        public bool IsEditing { get { return _isEditing; } private set { _isEditing = value; } }
        private Utils utils = new Utils();

        public SelectObjectTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTool) : base(axMap, mapControlTool)
        {
            
        }

        private void AddLayerToSelectionList(ILayer selectionLayer)
        {
            CurrentSelectionLayers.Add(selectionLayer);

            Shapefile sf = AxMap.get_Shapefile(selectionLayer.Handle);
            sf.Identifiable = true;
            sf.Selectable = true;
            sf.SelectionColor = utils.ColorByName(tkMapColor.Yellow);
            
        }

        private void UnselectAllLayer()
        {
            foreach (ILayer layer in MapControlTools.Layers)
            {
                Shapefile sf = AxMap.get_Shapefile(layer.Handle);
                if (sf != null)
                {
                    sf.Identifiable = false;
                    sf.Selectable = false;
                    sf.SelectionColor = utils.ColorByName(tkMapColor.Yellow);

                    if (layer.GetType() == typeof(ResTBDamagePotentialLayer))
                    {
                        ResTBDamagePotentialLayer resTBDamagePotentialLayer = (ResTBDamagePotentialLayer)layer;

                        sf = AxMap.get_Shapefile(resTBDamagePotentialLayer.PointHandle);
                        sf.Identifiable = false;
                        sf.Selectable = false;
                        sf = AxMap.get_Shapefile(resTBDamagePotentialLayer.LineHandle);
                        sf.Identifiable = false;
                        sf.Selectable = false;
                        sf = AxMap.get_Shapefile(resTBDamagePotentialLayer.PolygonHandle);
                        sf.Identifiable = false;
                        sf.Selectable = false;
                    }
                    else if (layer.GetType() == typeof(ResTBRiskMapLayer))
                    {
                        ResTBRiskMapLayer resTBRiskMapLayer = (ResTBRiskMapLayer)layer;

                        sf = AxMap.get_Shapefile(resTBRiskMapLayer.PointHandle);
                        sf.Identifiable = false;
                        sf.Selectable = false;
                        sf = AxMap.get_Shapefile(resTBRiskMapLayer.LineHandle);
                        sf.Identifiable = false;
                        sf.Selectable = false;
                        sf = AxMap.get_Shapefile(resTBRiskMapLayer.PolygonHandle);
                        sf.Identifiable = false;
                        sf.Selectable = false;
                    }
                }
            }
        }
        
        public bool StartSelecting(string layerName)
        {
            ILayer selectionLayer = MapControlTools.Layers.Where(m => m.Name == layerName).FirstOrDefault();
            if (selectionLayer == null)
                return false;
            CurrentSelectionLayers = new List<ILayer>();

            UnselectAllLayer();

            AddLayerToSelectionList(selectionLayer);

            IsEditing = true;
            AxMap.SendMouseDown = true;
            AxMap.SendMouseUp = true;

            AxMap.ShapeIdentified -= AxMap_ShapeIdentified;
            AxMap.ShapeIdentified += AxMap_ShapeIdentified;

            Events.MapControl_SelectingStateChange selectingStateChange = new Events.MapControl_SelectingStateChange() { SelectingState = Events.SelectingState.StartSelecting, SelectingLayers = CurrentSelectionLayers };
            On_SelectingStateChange(selectingStateChange);


            AxMap.Identifier.IdentifierMode = tkIdentifierMode.imAllLayers;
            AxMap.Identifier.OutlineColor = utils.ColorByName(tkMapColor.Yellow);
            AxMap.CursorMode = tkCursorMode.cmIdentify;
            

            return true;
        }
       

        public bool StartSelecting(Layer.ResTBPostGISType type)
        {

            CurrentSelectionLayers = new List<ILayer>();

            UnselectAllLayer();

            foreach (ILayer layer in MapControlTools.Layers)
            {
                if (layer.GetType() == typeof(ResTBDamagePotentialLayer))
                {
                    if (((ResTBPostGISLayer)layer).ResTBPostGISType == type)
                    {
                        ResTBDamagePotentialLayer resTBDamagePotentialLayer = (ResTBDamagePotentialLayer)layer;

                        CurrentSelectionLayers.Add(resTBDamagePotentialLayer);

                        Shapefile sf = AxMap.get_Shapefile(resTBDamagePotentialLayer.PointHandle);
                        sf.Identifiable = true;
                        sf.Selectable = true;
                        sf = AxMap.get_Shapefile(resTBDamagePotentialLayer.LineHandle);
                        sf.Identifiable = true;
                        sf.Selectable = true;
                        sf = AxMap.get_Shapefile(resTBDamagePotentialLayer.PolygonHandle);
                        sf.Identifiable = true;
                        sf.Selectable = true;
                    }                    
                }
                else if (layer.GetType().BaseType == typeof(ResTBPostGISLayer))
                {
                    if (((ResTBPostGISLayer)layer).ResTBPostGISType == type)
                    {
                        AddLayerToSelectionList(layer);
                    }
                }
            }

            IsEditing = true;
            AxMap.SendMouseDown = true;
            AxMap.SendMouseUp = true;

            AxMap.ShapeIdentified -= AxMap_ShapeIdentified;
            AxMap.ShapeIdentified += AxMap_ShapeIdentified;

            Events.MapControl_SelectingStateChange selectingStateChange = new Events.MapControl_SelectingStateChange() { SelectingState = Events.SelectingState.StartSelecting, SelectingLayers = CurrentSelectionLayers };
            On_SelectingStateChange(selectingStateChange);

            AxMap.Identifier.IdentifierMode = tkIdentifierMode.imAllLayers;
            AxMap.Identifier.OutlineColor = utils.ColorByName(tkMapColor.Yellow);
            AxMap.CursorMode = tkCursorMode.cmIdentify;

            return true;
        }

        public bool StartSelecting(DB.Models.NatHazard natHazard, bool beforeMeasure)
        {
            CurrentSelectionLayers = new List<ILayer>();

            UnselectAllLayer();

            foreach (ILayer layer in MapControlTools.Layers)
            {
                if (layer.GetType() == typeof(ResTBHazardMapLayer))
                {
                    if (beforeMeasure)
                    {
                        if ((((ResTBHazardMapLayer)layer).NatHazard.ID == natHazard.ID) && (((ResTBHazardMapLayer)layer).ResTBPostGISType == ResTBPostGISType.HazardMapBefore))
                        {
                            AddLayerToSelectionList(layer);
                        }
                    }
                    else
                    {
                        if ((((ResTBHazardMapLayer)layer).NatHazard.ID == natHazard.ID) && (((ResTBHazardMapLayer)layer).ResTBPostGISType == ResTBPostGISType.HazardMapAfter))
                        {
                            AddLayerToSelectionList(layer);
                        }
                    }
                }
            }

            IsEditing = true;
            AxMap.SendMouseDown = true;
            AxMap.SendMouseUp = true;

            AxMap.ShapeIdentified -= AxMap_ShapeIdentified;
            AxMap.ShapeIdentified += AxMap_ShapeIdentified;

            Events.MapControl_SelectingStateChange selectingStateChange = new Events.MapControl_SelectingStateChange() { SelectingState = Events.SelectingState.StartSelecting, SelectingLayers = CurrentSelectionLayers };
            On_SelectingStateChange(selectingStateChange);

            AxMap.Identifier.IdentifierMode = tkIdentifierMode.imAllLayers;
            AxMap.Identifier.OutlineColor = utils.ColorByName(tkMapColor.Yellow);
            AxMap.CursorMode = tkCursorMode.cmIdentify;

            return true;
        }

        public void StopSelecting()
        {
            IsEditing = false;
            if (CurrentSelectionLayers != null)
            {
                foreach (ILayer layer in CurrentSelectionLayers)
                {
                    Shapefile sf = AxMap.get_Shapefile(layer.Handle);
                    sf.SelectNone();

                    if (layer.GetType() == typeof(ResTBDamagePotentialLayer))
                    {
                        
                            ResTBDamagePotentialLayer resTBDamagePotentialLayer = (ResTBDamagePotentialLayer)layer;
                       

                            sf = AxMap.get_Shapefile(resTBDamagePotentialLayer.PointHandle);
                        sf.SelectNone();
                            sf = AxMap.get_Shapefile(resTBDamagePotentialLayer.LineHandle);
                            sf.SelectNone();
                        sf = AxMap.get_Shapefile(resTBDamagePotentialLayer.PolygonHandle);
                            sf.SelectNone();

                    }
                }
            }

            CurrentSelectionLayers = null;
            //AxMap.ChooseLayer -= AxMap_ChooseLayer;
            AxMap.ShapeIdentified -= AxMap_ShapeIdentified;

            AxMap.CursorMode = tkCursorMode.cmNone;
            Events.MapControl_SelectingStateChange selectingStateChange = new Events.MapControl_SelectingStateChange() { SelectingState = Events.SelectingState.StopSelecting, SelectingLayers = CurrentSelectionLayers };
            On_SelectingStateChange(selectingStateChange);

        }

        public void ReStartSelecting()
        {
            UnselectAllLayer();

            foreach (ILayer selectionLayer in CurrentSelectionLayers)
            {
                if (selectionLayer.GetType() == typeof(ResTBDamagePotentialLayer))
                {
                    Shapefile sf = AxMap.get_Shapefile(((ResTBDamagePotentialLayer)selectionLayer).PointHandle);
                    sf.Identifiable = true;
                    sf.Selectable = true;
                    sf = AxMap.get_Shapefile(((ResTBDamagePotentialLayer)selectionLayer).PointHandle);
                    sf.Identifiable = true;
                    sf.Selectable = true;
                    sf = AxMap.get_Shapefile(((ResTBDamagePotentialLayer)selectionLayer).PointHandle);
                    sf.Identifiable = true;
                    sf.Selectable = true;
                }
                else
                {
                    Shapefile sf = AxMap.get_Shapefile(selectionLayer.Handle);
                    sf.Identifiable = true;
                    sf.Selectable = true;
                    sf.SelectionColor = utils.ColorByName(tkMapColor.Yellow);
                }
            }


        }

        private void AxMap_ShapeIdentified(object sender, AxMapWinGIS._DMapEvents_ShapeIdentifiedEvent e)
        {
            int selectedLayerHandle = e.layerHandle;
            int selectedShapeIndex = e.shapeIndex;

            // multiple layers clicked, for simplicity just take the point-type, because on others we can click somewhere
            if (e.layerHandle==-1)
            {
                SelectionList sl = AxMap.IdentifiedShapes;
                for (int i=0; i<sl.Count; i++)
                {
                    Shapefile sf = AxMap.get_Shapefile(sl.LayerHandle[i]);
                    if (sf.ShapefileType == ShpfileType.SHP_POINT)
                    {
                        selectedLayerHandle = sl.LayerHandle[i];
                        selectedShapeIndex = sl.ShapeIndex[i];
                    }
                    else
                    {
                        sl.RemoveByLayerHandle(sl.LayerHandle[i]);
                    }
                }
                // if there was no point, just take the first one
                if (selectedLayerHandle==-1) {
                    selectedLayerHandle = sl.LayerHandle[0];
                    selectedShapeIndex = sl.ShapeIndex[0];
                }


            }

            foreach (ILayer layer in CurrentSelectionLayers)
            {
                if (layer.GetType() == typeof(ResTBDamagePotentialLayer))
                {
                    if ((selectedLayerHandle == ((ResTBDamagePotentialLayer)layer).PointHandle) || (selectedLayerHandle == ((ResTBDamagePotentialLayer)layer).LineHandle) || (selectedLayerHandle == ((ResTBDamagePotentialLayer)layer).PolygonHandle))
                    {
                        Events.MapControl_Clicked clicked = new Events.MapControl_Clicked();
                        Shapefile sf = AxMap.get_Shapefile(selectedLayerHandle);

                        clicked.ClickedShape = sf.Shape[selectedShapeIndex];
                        clicked.ClickedShapeFile = sf;
                        clicked.ClickedShapeIndex = selectedShapeIndex;
                        clicked.Layer = layer;
                        MapControlTools.On_ShapeClicked(clicked);

                        Events.MapControl_SelectingStateChange selectingStateChange = new Events.MapControl_SelectingStateChange() { SelectingState = Events.SelectingState.ShapeSelected, SelectingLayers = CurrentSelectionLayers };
                        On_SelectingStateChange(selectingStateChange);

                        //StopSelecting();
                    }
                }
                else if (selectedLayerHandle == layer.Handle)
                {
                    Events.MapControl_Clicked clicked = new Events.MapControl_Clicked();
                    Shapefile sf = AxMap.get_Shapefile(selectedLayerHandle);

                    clicked.ClickedShape = sf.Shape[selectedLayerHandle];
                    clicked.ClickedShapeFile = sf;
                    clicked.ClickedShapeIndex = selectedShapeIndex;
                    clicked.Layer = layer;
                    MapControlTools.On_ShapeClicked(clicked);
                    Events.MapControl_SelectingStateChange selectingStateChange = new Events.MapControl_SelectingStateChange() { SelectingState = Events.SelectingState.ShapeSelected, SelectingLayers = CurrentSelectionLayers };
                    On_SelectingStateChange(selectingStateChange);

                    //StopSelecting();
                }

                
            }

        }


        private void AxMap_ChooseLayer(object sender, AxMapWinGIS._DMapEvents_ChooseLayerEvent e)
        {
            if (CurrentSelectionLayers[0].GetType() == typeof(ResTBPostGISLayer))
            {
                e.layerHandle = ((ResTBPostGISLayer)CurrentSelectionLayers[0]).Handle;
            }
            else if (CurrentSelectionLayers[0].GetType() == typeof(ResTBHazardMapLayer))
            {
                e.layerHandle = ((ResTBHazardMapLayer)CurrentSelectionLayers[0]).Handle;
            }
            else
            {
                e.layerHandle = CurrentSelectionLayers[0].Handle;
            }
        }


    }
}
