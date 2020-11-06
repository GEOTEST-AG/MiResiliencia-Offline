using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Events
{
    public enum EditingState
    {
        None = 0,
        StartEditing = 1,
        AddShape = 2,
        EditSshape = 3,
        StopEditing = 4,
        SaveEditing = 5,
        DeleteShape = 6
        
    }
    public class MapControl_EditingStateChange : EventArgs
    {
        public EditingState EditingState { get; set; }
        public ResTB.Map.Layer.ILayer EditingLayer { get; set; }
    }
}
