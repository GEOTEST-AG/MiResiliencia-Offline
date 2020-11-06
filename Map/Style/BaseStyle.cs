using MapWinGIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Style
{
    public class BaseStyle : IStyle
    { 
        public Utils utils = new Utils();

        public virtual void createStyle(Shapefile sf)
        {
        }

        public void setStyleForLayer(AxMapWinGIS.AxMap axMap, int layerHandle)
        {
                Shapefile sf = axMap.get_OgrLayer(layerHandle).GetBuffer();
                createStyle(sf);
     
        }

    }
}
