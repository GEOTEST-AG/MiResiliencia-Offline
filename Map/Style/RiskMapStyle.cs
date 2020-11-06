using MapWinGIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Style
{
    public class RiskMapStyle : BaseStyle, IStyle
    {
        public override void createStyle(Shapefile sf)
        {

            ShapeDrawingOptions options = sf.DefaultDrawingOptions;
            if (sf.ShapefileType==ShpfileType.SHP_POLYLINE) sf.DefaultDrawingOptions.LineWidth = 4;
            sf.DefaultDrawingOptions.PointSize = 15;
            int fieldIndex = sf.Table.FieldIndexByName["PropertyDamage"];
            sf.Categories.Generate(fieldIndex, tkClassificationType.ctNaturalBreaks, 3);
            sf.Categories.ApplyExpressions();
            ColorScheme scheme = new ColorScheme();
            scheme.SetColors2(tkMapColor.White, tkMapColor.Red);
            sf.Categories.ApplyColorScheme(tkColorSchemeType.ctSchemeGraduated, scheme);

            
        }
    }
}
