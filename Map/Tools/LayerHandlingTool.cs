using ResTB.Map.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Tools
{
    public enum LayerMoveType
    {
        TOP,
        BOTTOM,
        UP,
        DOWN,
    }

    public class LayerHandlingTool : BaseTool
    {
        public LayerHandlingTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTool) : base(axMap, mapControlTool)
        {

        }

        public List<string> GetLayerNamesFromPostGISType(ResTBPostGISType type)
        {
            return MapControlTools.Layers.Where(m => m.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m).ResTBPostGISType == type).Select(m => m.Name).ToList();
        }

        public bool RemoveLayer(string name)
        {
            ILayer removeLayer = MapControlTools.Layers.Where(m => m.Name == name).FirstOrDefault();
            if (removeLayer == null)
                return false;

            if (removeLayer.GetType() == typeof(ResTBDamagePotentialLayer))
            {
                AxMap.RemoveLayer(((ResTBDamagePotentialLayer)removeLayer).PointHandle);
                AxMap.RemoveLayer(((ResTBDamagePotentialLayer)removeLayer).LineHandle);
                AxMap.RemoveLayer(((ResTBDamagePotentialLayer)removeLayer).PolygonHandle);
            }
            else if (removeLayer.GetType() == typeof(ResTBRiskMapLayer))
            {
                AxMap.RemoveLayer(((ResTBRiskMapLayer)removeLayer).PointHandle);
                AxMap.RemoveLayer(((ResTBRiskMapLayer)removeLayer).LineHandle);
                AxMap.RemoveLayer(((ResTBRiskMapLayer)removeLayer).PolygonHandle);
            }
            else AxMap.RemoveLayer(removeLayer.Handle);
            if (MapControlTools.EditingTool.CurrentEditingLayer?.GetType() == typeof(ResTBPostGISLayer))
            {
                AxMap.RemoveLayer(((ResTBPostGISLayer)MapControlTools.EditingTool.CurrentEditingLayer).EditingLayerHandle);
            }
            MapControlTools.Layers.Remove(removeLayer);
            Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.RemoveLayer, Layer = removeLayer };
            On_LayerChange(layerchange);
            return true;
        }


        public bool RemoveAllLayers()
        {
            AxMap.RemoveAllLayers();
            MapControlTools.Layers.Clear();
            //MapControlTools.EditingTool.CurrentEditingLayer = null;
            Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.RemoveLayer, Layer = null };
            On_LayerChange(layerchange);
            return true;
        }

        public bool ZoomToLayer(string name)
        {
            ILayer zoomToLayer = MapControlTools.Layers.Where(m => m.Name == name).FirstOrDefault();
            if (zoomToLayer == null)
                return false;
            if (zoomToLayer.GetType() == typeof(ResTBDamagePotentialLayer))
            {
                AxMap.ZoomToLayer(((ResTBDamagePotentialLayer)zoomToLayer).PointHandle);
            }
            else if (zoomToLayer.GetType() == typeof(ResTBRiskMapLayer))
            {
                AxMap.ZoomToLayer(((ResTBRiskMapLayer)zoomToLayer).PointHandle);
            }
            else
                AxMap.ZoomToLayer(zoomToLayer.Handle);
            return true;
        }

        private int ShapesCount(int handle)
        {
            var sf = AxMap.get_Shapefile(handle);
            if (sf != null)
            {
                // NULL Shapes are returned as 1. Should we return 0 if it is a NULL-Shape?

                if (sf.NumShapes == 1)
                {
                    var shapetype = sf.Shape[0].ShapeType;
                    if (shapetype == MapWinGIS.ShpfileType.SHP_NULLSHAPE) return 0;
                }

                return sf.NumShapes;
            }
            else
                return 0;
        }

        public int ShapesCount(string name)
        {
            
            ILayer countLayer = MapControlTools.Layers.Where(m => m.Name == name).FirstOrDefault();
            if (countLayer == null)
                return 0;

            if (countLayer.GetType() == typeof(ResTBDamagePotentialLayer))
            {
                int sumOfShapes = 0;
                sumOfShapes += ShapesCount(((ResTBDamagePotentialLayer)countLayer).PointHandle);
                sumOfShapes += ShapesCount(((ResTBDamagePotentialLayer)countLayer).LineHandle);
                sumOfShapes += ShapesCount(((ResTBDamagePotentialLayer)countLayer).PolygonHandle);
                return sumOfShapes;
            }
            else if (countLayer.GetType() == typeof(ResTBRiskMapLayer))
            {
                int sumOfShapes = 0;
                sumOfShapes += ShapesCount(((ResTBRiskMapLayer)countLayer).PointHandle);
                sumOfShapes += ShapesCount(((ResTBRiskMapLayer)countLayer).LineHandle);
                sumOfShapes += ShapesCount(((ResTBRiskMapLayer)countLayer).PolygonHandle);
                return sumOfShapes;
            }

            return ShapesCount(countLayer.Handle);
            
        }


        public bool SetLayerPosition(string name, LayerMoveType layerMoveType)
        {
            ILayer layer = MapControlTools.Layers.Where(m => m.Name == name).FirstOrDefault();
            if (layer == null)
                return false;

            // TODO: Handle Damage Potential Layer
            //if (layer.GetType() == typeof(ResTBDamagePotentialLayer))
            //{
            //  int currentPosition = MapControlTools.AxMap.get_LayerPosition(((ResTBDamagePotentialLayer)layer).PointHandle);
            //}

            int currentPosition = MapControlTools.AxMap.get_LayerPosition(layer.Handle);
            bool rc = false;

            switch (layerMoveType)
            {
                case LayerMoveType.TOP:
                    rc = MapControlTools.AxMap.MoveLayerTop(currentPosition);
                    break;
                case LayerMoveType.BOTTOM:
                    rc = MapControlTools.AxMap.MoveLayerBottom(currentPosition);
                    break;
                case LayerMoveType.UP:
                    rc = MapControlTools.AxMap.MoveLayerUp(currentPosition);
                    break;
                case LayerMoveType.DOWN:
                    rc = MapControlTools.AxMap.MoveLayerDown(currentPosition);
                    break;
                default:
                    break;
            }

            if (rc)
            {
                Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.ChangeOrder, Layer = layer };
                On_LayerChange(layerchange);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SetLayerVisible(string name, bool visible)
        {
            ILayer layer = MapControlTools.Layers.Where(m => m.Name == name).FirstOrDefault();
            if (layer == null)
                return false;

            if (layer.GetType() == typeof(ResTBDamagePotentialLayer))
            {
                MapControlTools.AxMap.set_LayerVisible(((ResTBDamagePotentialLayer)layer).PointHandle, visible);
                MapControlTools.AxMap.set_LayerVisible(((ResTBDamagePotentialLayer)layer).PolygonHandle, visible);
                MapControlTools.AxMap.set_LayerVisible(((ResTBDamagePotentialLayer)layer).LineHandle, visible);
            }
            else if (layer.GetType() == typeof(ResTBRiskMapLayer))
            {
                MapControlTools.AxMap.set_LayerVisible(((ResTBRiskMapLayer)layer).PointHandle, visible);
                MapControlTools.AxMap.set_LayerVisible(((ResTBRiskMapLayer)layer).PolygonHandle, visible);
                MapControlTools.AxMap.set_LayerVisible(((ResTBRiskMapLayer)layer).LineHandle, visible);
            }
            else
                MapControlTools.AxMap.set_LayerVisible(layer.Handle, visible);

            Events.MapControl_LayerChange layerchange = new Events.MapControl_LayerChange() { LayerChangeReason = Events.LayerChangeReason.ChangeVisibility, Layer = layer };
            On_LayerChange(layerchange);

            return true;
        }


    }
}
