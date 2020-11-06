using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapWinGIS;

namespace ResTB.Map.Style
{
    public class DamagePotentialStyle : BaseStyle, IStyle
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

            sf.DefaultDrawingOptions.PointSize = 12;

        }

        public override void createStyle(Shapefile sf)
        {
            LinePattern pattern = new LinePattern();
            pattern.AddLine(utils.ColorByName(tkMapColor.Black), 6.0f, tkDashStyle.dsSolid);
            pattern.AddLine(utils.ColorByName(tkMapColor.White), 5.0f, tkDashStyle.dsDot);


            ShapeDrawingOptions options = sf.DefaultDrawingOptions;


            // standard fill
            options.FillTransparency = 50;
            options.LineWidth = 3;
            options.LineVisible = true;
            options.LineColor = utils.ColorByName(tkMapColor.Blue);
            options.LineStipple = tkDashStyle.dsSolid;
            /*
            addExpression(sf, "Casa madera y/o adobe", "[Name] = \"Casa madera y/o adobe\"", "rgb(37, 49, 232)");
            addExpression(sf, "Casa ladrillo", "[Name] = \"Casa ladrillo\"", "rgb(13, 22, 168)");
            addExpression(sf, "Escuela", "[Name] = \"Escuela estándar (muros de concreto)\"", "rgb(19, 25, 117)");
            addExpression(sf, "Iglesia", "[Name] = \"Iglesia\"", "rgb(80, 88, 178)");
            addExpression(sf, "Colegio", "[Name] = \"Colegio estatal\"", "rgb(50, 92, 163)");
            addExpression(sf, "Tanque", "[Name] = \"Tanque de agua\"", "rgb(33, 91, 188)");
            addExpression(sf, "Molino", "[Name] = \"Molino\"", "rgb(25, 66, 135)");
            addExpression(sf, "Taller", "[Name] = \"Taller de bicicleta\"", "rgb(57, 66, 168)");
            addExpression(sf, "Adminis", "[Name] = \"Edificio de administración\"", "rgb(65, 51, 155)");
            addExpression(sf, "Mercado", "[Name] = \"Mercado\"", "rgb(26, 34, 127)");
            addExpression(sf, "Hospital", "[Name] = \"Hospital\"", "rgb(57, 62, 119)");
            addExpression(sf, "Escuela principal", "[Name] = \"Escuela principal\"", "rgb(40, 96, 186)");
            addExpression(sf, "Puesto", "[Name] = \"Puesto de salud\"", "rgb(96, 36, 153)");

            addExpression(sf, "Infraestructura", "[Name] = \"Infraestructura de comunicación\"", "rgb(74, 39, 107)");
            addExpression(sf, "Tubería", "[Name] = \"Tubería de agua al aire libre\"", "rgb(60, 22, 96)");
            addExpression(sf, "Aeropuerto", "[Name] = \"Terminal / Aeropuerto\"", "rgb(63, 11, 112)");

            addExpression(sf, "Carretera", "[Name] = \"Carretera principal (pavimentada)\"", "rgb(103, 152, 198)");
            addExpression(sf, "Camino", "[Name] = \"Camino comunitario (para vehiculos, no pavimentado)\"", "rgb(51, 97, 140)");
            addExpression(sf, "Puente", "[Name] = \"Puente comunitario (para vehiculos, no pavimentado)\"", "rgb(10, 48, 84)");
            addExpression(sf, "electrica", "[Name] = \"Línea electrica (incluyendo postes)\"", "rgb(40, 134, 224)");
            addExpression(sf, "Transformador", "[Name] = \"Transformador\"", "rgb(31, 110, 186)");
            addExpression(sf, "Reservorio", "[Name] = \"Reservorio (incluyendo bombas)\"", "rgb(31, 152, 168)");
            addExpression(sf, "Canal", "[Name] = \"Canal de riego\"", "rgb(26, 194, 216)");
            addExpression(sf, "Huerto", "[Name] = \"Huerto\"", "rgb(17, 139, 155)");
            addExpression(sf, "Campos", "[Name] = \"Campos de maíz, frijol, sorghum\"", "rgb(14, 167, 178)");
            addExpression(sf, "Frutales", "[Name] = \"Frutales\"", "rgb(36, 133, 140)");
            addExpression(sf, "Camaroneras", "[Name] = \"Camaroneras\"", "rgb(57, 62, 119)");

            addExpression(sf, "Pastos", "[Name] = \"Pastos, Pastizales\"", "rgb(25, 66, 135)");
            */
            addExpression(sf, "NotStandard", "[IsStandard] = 0", "rgb(255, 0, 0)");

            sf.Categories.ApplyExpressions();
        }
    }
}
