using System;
using MapWinGIS;
using ResTB.Map.Style;

namespace ResTB.Map.Layer
{
    public class BaseLayer : ILayer
    {
        public int Handle { get; set; }
        public string Name { get; set; }
        public string ExportImportFileName { get; set; }
        public LayerType LayerType { get; set; }
        public IStyle Style { get { return new EmptyStyle(); } set { } }

        public int ShapeCount { get; set; }
        public int LayerPosition { get; set; }
        public bool IsVisible { get; set; }
        

        public virtual void ApplyStyle(AxMapWinGIS.AxMap AxMap)
        {
            
        }

        public virtual object GetObjectFromShape(Shapefile shapefile, int index)
        {
            return null;
        }
    }
}
