using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapWinGIS;

namespace ResTB.Map.Style
{
    public class PerimeterStyle : BaseStyle, IStyle
    {
        public override void createStyle(Shapefile sf)
        {

            ShapeDrawingOptions options = sf.DefaultDrawingOptions;
            // standard fill
            options.FillTransparency = 20;
            options.LineWidth = 3;
            options.LineVisible = true;
            options.LineStipple = tkDashStyle.dsDash;
            options.LineColor = utils.ColorByName(tkMapColor.Black);
        }
    }
}
