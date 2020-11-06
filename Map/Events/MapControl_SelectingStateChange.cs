using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Events
{
    public enum SelectingState
    {
        None = 0,
        StartSelecting = 1,
        ShapeSelected = 2,
        StopSelecting = 3
        
    }
    public class MapControl_SelectingStateChange : EventArgs
    {
        public SelectingState SelectingState { get; set; }
        public List<ResTB.Map.Layer.ILayer> SelectingLayers { get; set; }
    }
}
