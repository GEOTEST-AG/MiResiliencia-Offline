using ResTB.Map.Style;

namespace ResTB.Map.Layer
{
    public enum LayerType
    {
        ProjectLayer = 0,
        BaseLayer = 1,
        CustomLayerWMS = 2,
        CustomLayerRaster = 3,
        CustomLayerSHP = 4
    }

    public interface ILayer
    {
        int Handle { get; set; }
        string Name { get; set; }
        LayerType LayerType { get; set; }
        IStyle Style { get; set; }

        int ShapeCount { get; set; }
        int LayerPosition { get; set; }
        bool IsVisible { get; set; }

    }


}
