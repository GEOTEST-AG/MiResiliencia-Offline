using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapWinGIS;

namespace ResTB.Map.Style
{
    public class MitigationStyle : BaseStyle, IStyle
    {
        public override void createStyle(Shapefile sf)
        {
            LinePattern pattern = new LinePattern();
            pattern.AddLine(utils.ColorByName(tkMapColor.Black), 6.0f, tkDashStyle.dsSolid);
            pattern.AddLine(utils.ColorByName(tkMapColor.White), 5.0f, tkDashStyle.dsDot);

            ShapeDrawingOptions options = sf.DefaultDrawingOptions;
            // standard fill

            options.FillColor = utils.ColorByName(tkMapColor.Gray);
            options.LineWidth = 2;
            options.LinePattern = pattern;
            options.UseLinePattern = true;
            options.LineVisible = true;
            options.LineColor = utils.ColorByName(tkMapColor.LightYellow);
        }
    }
}
