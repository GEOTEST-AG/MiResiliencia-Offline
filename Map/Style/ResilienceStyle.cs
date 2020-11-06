using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapWinGIS;

namespace ResTB.Map.Style
{
    public class ResilienceStyle : BaseStyle, IStyle
    {
        private void addExpression(Shapefile sf, string name, string expression, string rgb)
        {
            // rgb example: rgb(37, 49, 232)
            rgb = rgb.Replace("rgb(", "").Replace(" ", "").Replace(")", "");
            string[] rgbv = rgb.Split(',');


            ShapefileCategory ct = sf.Categories.Add(name);
            ct.Expression = expression;
            System.Drawing.Color co = Color.FromArgb(50, Int32.Parse(rgbv[0]), Int32.Parse(rgbv[1]), Int32.Parse(rgbv[2]));
            ct.DrawingOptions.FillColor = Convert.ToUInt32(ColorTranslator.ToOle(co));
            ct.DrawingOptions.LineColor = Convert.ToUInt32(ColorTranslator.ToOle(co));

        }

        public override void createStyle(Shapefile sf)
        {
            LinePattern pattern = new LinePattern();
            pattern.AddLine(utils.ColorByName(tkMapColor.Black), 6.0f, tkDashStyle.dsSolid);

            ShapeDrawingOptions options = sf.DefaultDrawingOptions;


            // standard fill
            options.FillTransparency = 60;
            options.LineWidth = 2;
            options.LineVisible = true;
            options.LineColor = utils.ColorByName(tkMapColor.Black);
            options.LineStipple = tkDashStyle.dsSolid;

            addExpression(sf, "With Resilience", "[savedvalues] > 1", "rgb(194, 66, 244)");
            addExpression(sf, "With Resilience", "[savedvalues] = 1", "rgb(65, 160, 244)");

            sf.Categories.ApplyExpressions();
        }
    }
}
