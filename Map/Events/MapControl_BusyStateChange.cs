using MapWinGIS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Events
{
    public enum BusyState
    {
        Idle = 0,
        Busy = 1,
        BackgroundBusy = 2
    }
    public class MapControl_BusyStateChange : EventArgs
    {
        public BusyState BusyState { get; set; }
        public string KeyOfSender { get; set; }
        public int Percent { get; set; }
        public string Message { get; set; }
    }
}
