using MapWinGIS;
using ResTB.Map.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Layer
{
    class WmsLayerLayer : BaseLayer, ILayer
    {
        public WmsLayer WmsLayerObj { get; set; }
    }
}
