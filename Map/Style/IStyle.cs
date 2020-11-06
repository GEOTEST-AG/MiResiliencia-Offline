using MapWinGIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Style
{
    public interface IStyle
    {
        void createStyle(Shapefile sf);
        void setStyleForLayer(AxMapWinGIS.AxMap axMap, int layerHandle);

    }
}
