using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Events
{
    public enum LayerChangeReason
    {
        AddLayer = 1,
        RemoveLayer = 2,
        ChangeVisibility = 3,
        ChangeOrder = 4,
        EditedLayer = 5
        
    }
    public class MapControl_LayerChange : EventArgs
    {
        public LayerChangeReason LayerChangeReason { get; set; }
        public ResTB.Map.Layer.ILayer Layer { get; set; }
    }
}
