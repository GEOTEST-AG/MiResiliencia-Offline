using MapWinGIS;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using ResTB.DB.Models;
using ResTB.Map.Layer;
using ResTB.Translation.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResTB.Map.Tools
{
    public class PrintTool : BaseTool
    {
        private string filename;
        private Project project;

        public PrintTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTool) : base(axMap, mapControlTool)
        {
        }

        public void SaveMapAsJPEG(string filename)
        {
            var gdalUtils = new GdalUtils();
            
            PrintCallback callback = new PrintCallback(MapControlTools);
            gdalUtils.GlobalCallback = callback;

            var image = AxMap.SnapShot(AxMap.Extents);
            image.Save(filename, true, ImageType.JPEG_FILE);
        }

        public void PrintAsPDF(string filename, Project project)
        {
            // first, just start beginning of tile downloading...

            string localdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ResTBDesktop";
            PdfDocument mapTemplate = PdfReader.Open(localdata + "\\PrintTemplates\\A4_landscape.pdf", PdfDocumentOpenMode.Modify);
            PdfDocument outputDocument = new PdfDocument();
            outputDocument.PageLayout = mapTemplate.PageLayout;
            PdfPage page = mapTemplate.Pages[0];
            page = outputDocument.AddPage();
            page.Orientation = mapTemplate.Pages[0].Orientation;
            page.Width = mapTemplate.Pages[0].Width;
            page.Height = mapTemplate.Pages[0].Height;
            int dx = (int)page.Width.Point, dy = (int)page.Height.Point;
            // calculate aspect
            var diffX = AxMap.Extents.xMax - AxMap.Extents.xMin;
            double aspect = ((double)dy / dx);
            int diffY = (int)(aspect * diffX);

            // start tile loading for cache
            Extents MapExtents = new Extents();
            MapExtents.SetBounds(AxMap.Extents.xMin, AxMap.Extents.yMin, AxMap.Extents.zMin, AxMap.Extents.xMax, AxMap.Extents.yMin + diffY, AxMap.Extents.zMax);

            AxMap.TilesLoaded += AxMap_TilesLoaded;
            int width = (int)(dx * (96.0 / 72.0) * 1.5);
            this.filename = filename;
            this.project = project;
            bool shouldwait = AxMap.LoadTilesForSnapshot(MapExtents, (int)(dx * (96.0 / 72.0)*2), "Snapshot");
            if (shouldwait)
            {
                AxMap.TilesLoaded -= AxMap_TilesLoaded;
                PrintAsPDFAfterTilesAreLoaded();
            }            

        }

        private void changeFont(PdfTextField textfield, int size)
        {
            if (textfield.Elements.ContainsKey("/DA") == false)
            {
                textfield.Elements.Add("/DA", new PdfString("/CoBo " + size + " Tf 0 g"));
            }
            else
            {
                textfield.Elements["/DA"] = new PdfString("/CoBo " + size + " Tf 0 g");
            }
        }

        private void PrintAsPDFAfterTilesAreLoaded()
        {
            AxMap.TilesLoaded -= AxMap_TilesLoaded;
            string localdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ResTBDesktop";
            string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ResTBDesktop";
            PdfDocument mapTemplate = PdfReader.Open(localappdata + "\\PrintTemplates\\A4_landscape.pdf", PdfDocumentOpenMode.Modify);
            PdfDocument outputDocument = new PdfDocument();
            outputDocument.PageLayout = mapTemplate.PageLayout;

            PdfPage page = outputDocument.AddPage();
            page.Orientation = mapTemplate.Pages[0].Orientation;
            page.Width = mapTemplate.Pages[0].Width;
            page.Height = mapTemplate.Pages[0].Height;

            int dx = (int)page.Width.Point, dy = (int)page.Height.Point;
            // calculate aspect
            var diffX = AxMap.Extents.xMax - AxMap.Extents.xMin;
            double aspect = ((double)dy / dx);
            int diffY = (int)(aspect * diffX);

            // start tile loading for cache
            Extents MapExtents = new Extents();
            MapExtents.SetBounds(AxMap.Extents.xMin, AxMap.Extents.yMin, AxMap.Extents.zMin, AxMap.Extents.xMax, AxMap.Extents.yMin + diffY, AxMap.Extents.zMax);


            // scale
            double meter = AxMap.GeodesicDistance(AxMap.Extents.xMin, AxMap.Extents.yMin, AxMap.Extents.xMax, AxMap.Extents.yMin);
            double pageWidthInMeter = ((page.Width / 72) * 2.54) / 100;
            int scale = (int)(meter / pageWidthInMeter);

            int scaleRounded = scale % 100 >= 50 ? scale + 100 - scale % 100 : scale - scale % 100;
            if ((scale - scaleRounded < 10) && (scale - scaleRounded > -10)) scale = scaleRounded;

            // Load the template stuff and change the acroforms...        


            PdfAcroForm acroform = mapTemplate.AcroForm;
            if (acroform.Elements.ContainsKey("/NeedAppearances"))
            {
                acroform.Elements["/NeedAppearances"] = new PdfSharp.Pdf.PdfBoolean(true);
            }
            else
            {
                acroform.Elements.Add("/NeedAppearances", new PdfSharp.Pdf.PdfBoolean(true));
            }

            var name = (PdfTextField)(acroform.Fields["ProjectTitle"]);
            changeFont(name, 12);
            name.Value = new PdfString(project.Name);

            var numberlabel = (PdfTextField)(acroform.Fields["ProjectNumberLabel"]);
            changeFont(numberlabel, 7);
            numberlabel.Value = new PdfString(Resources.Project_Number);
            var number = (PdfTextField)(acroform.Fields["ProjectNumber"]);
            changeFont(number, 7);
            number.Value = new PdfString(project.Number);

            var descriptionlabel = (PdfTextField)(acroform.Fields["DescriptionLabel"]);
            changeFont(descriptionlabel, 7);
            descriptionlabel.Value = new PdfString(Resources.Description);
            var description = (PdfTextField)(acroform.Fields["Description"]);
            changeFont(description, 7);
            description.Value = new PdfString(project.Description);

            var scalefield = (PdfTextField)(acroform.Fields["Scale"]);
            changeFont(scalefield, 10);
            scalefield.Value = new PdfString("1 : " + (int)scale);

            var legend = (PdfTextField)(acroform.Fields["LegendLabel"]);
            legend.Value = new PdfString(Resources.Legend);
            var copyright = (PdfTextField)(acroform.Fields["CopyrightLabel"]);
            copyright.Value = new PdfString("Impreso con " +  Resources.App_Name);

            mapTemplate.Flatten();
            mapTemplate.Save(localdata + "\\printtemp.pdf");

           mapTemplate.Close();

      

            

            

            XGraphics gfx = XGraphics.FromPdfPage(page);

            var imageFromMap = AxMap.SnapShot3(AxMap.Extents.xMin, AxMap.Extents.xMax, AxMap.Extents.yMin + diffY, AxMap.Extents.yMin, (int)(dx * (96.0 / 72.0) * 2));
            imageFromMap.Save(localdata + "\\printTemp.tif", false, ImageType.TIFF_FILE);

            XImage image = XImage.FromFile(localdata + "\\printTemp.tif");
            // Left position in point
            double x = (dx - image.PixelWidth * 72 / image.HorizontalResolution) / 2;
            gfx.DrawImage(image, 0, 0, dx, dy);

            /*
            XImage mapLayout = XImage.FromFile(localdata + "\\PrintTemplates\\A4_quer.png");


            double width = mapLayout.PixelWidth * 72 / mapLayout.HorizontalResolution;
            double height = mapLayout.PixelHeight * 72 / mapLayout.HorizontalResolution;

            gfx.DrawImage(mapLayout, (dx - width) / 2, (dy - height) / 2, width, height);
            */
            //outputDocument.AddPage(mapTemplateFilledOut.Pages[0]);


            XPdfForm form = XPdfForm.FromFile(localdata + "\\printtemp.pdf" );
            gfx.DrawImage(form, 0, 0);

            outputDocument.Save(filename);
            image.Dispose();
            Process.Start(filename);
        }

        private void AxMap_TilesLoaded(object sender, AxMapWinGIS._DMapEvents_TilesLoadedEvent e)
        {
            if (e.snapShot)
            {
                PrintAsPDFAfterTilesAreLoaded();
            }
        }
    }

    class PrintCallback : ICallback
    {

        MapControlTools MapControlTool { get; set; }
        public event EventHandler GdalFinished;

        public PrintCallback(MapControlTools mapControlTool) : base()
        {
            this.MapControlTool = mapControlTool;

        }
        public void Error(string KeyOfSender, string ErrorMsg)
        {

            Events.MapControl_Error export_error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.ImportExportError, InMethod = "ExportProject", AxMapError = ErrorMsg };
            MapControlTool.On_Error(export_error);
        }
        public void Progress(string KeyOfSender, int Percent, string Message)
        {
            if (Percent == 100) On_GdalFinished(new EventArgs());
        }

        public virtual void On_GdalFinished(EventArgs e)
        {
            GdalFinished?.Invoke(this, e);
        }

    }
}
