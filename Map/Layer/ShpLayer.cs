using MapWinGIS;
using ResTB.Map.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Layer
{
    public class ShpLayer : BaseLayer, ILayer
    {
        public Shapefile Shapefile { get; set; }
    }
}
