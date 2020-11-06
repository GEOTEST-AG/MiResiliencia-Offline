using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapWinGIS;

namespace ResTB.Map.Style
{
    public class HazardMapStyle : BaseStyle, IStyle
    {
        public override void createStyle(Shapefile sf)
        {
            LinePattern pattern = new LinePattern();
            pattern.AddLine(utils.ColorByName(tkMapColor.Black), 6.0f, tkDashStyle.dsSolid);
            pattern.AddLine(utils.ColorByName(tkMapColor.White), 5.0f, tkDashStyle.dsDot);


            ShapeDrawingOptions options = sf.DefaultDrawingOptions;
            sf.Labels.Generate("[Index]", tkLabelPositioning.lpInteriorPoint, false);
            sf.Labels.FontOutlineColor = utils.ColorByName(tkMapColor.Black);
            sf.Labels.FontOutlineVisible = true;
            sf.Labels.FontSize = 12;
            sf.Labels.FontColor = utils.ColorByName(tkMapColor.White);
            sf.Labels.FrameVisible = false;
            // standard fill
            options.FillTransparency = 70;
            options.LineWidth = 2;
            options.LineVisible = true;
            options.LineColor = utils.ColorByName(tkMapColor.Black);
            options.LineStipple = tkDashStyle.dsSolid;


            ShapefileCategory ct = sf.Categories.Add("Danger");
            ct.Expression = "[Index] >= 7";
            ct.DrawingOptions.FillColor = utils.ColorByName(tkMapColor.Red);


            ct = sf.Categories.Add("Medium");
            ct.Expression = "[Index] < 7";
            ct.DrawingOptions.FillColor = utils.ColorByName(tkMapColor.Yellow);


            ct = sf.Categories.Add("Low");
            ct.Expression = "[Index] < 3";
            ct.DrawingOptions.FillColor = utils.ColorByName(tkMapColor.Green);

            sf.Categories.ApplyExpressions();
        }
    }
}
