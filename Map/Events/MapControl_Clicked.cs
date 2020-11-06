using MapWinGIS;
using ResTB.Map.Layer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Events
{
    public class MapControl_Clicked : EventArgs
    {
        public Shape ClickedShape { get; set; }
        public Shapefile ClickedShapeFile { get; set; }
        public int ClickedShapeIndex { get; set; }
        public ResTB.Map.Layer.ILayer Layer { get; set; }

        public object GetClickedDBObject()
        {
            return ((BaseLayer)Layer).GetObjectFromShape(ClickedShapeFile, ClickedShapeIndex);
        }
    }
}
