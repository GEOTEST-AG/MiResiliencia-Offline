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
    public class MapControl_RasterReprojected : EventArgs
    {
        public Layer.RasterLayer rasterLayer { get; set; }
    }
}
