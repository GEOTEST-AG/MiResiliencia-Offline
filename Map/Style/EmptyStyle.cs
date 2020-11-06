using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapWinGIS;

namespace ResTB.Map.Style
{
    public class EmptyStyle : BaseStyle, IStyle
    {
        public override void createStyle(Shapefile sf)
        {
           
        }
    }
}
