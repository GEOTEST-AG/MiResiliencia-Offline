using MapWinGIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ResTB.Map
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        private AxMapWinGIS.AxMap axMap1;
        private MapControlTools _tools;

        public MapControlTools Tools { get { return _tools; } }

        public MapControl()
        {
            InitializeComponent();
        }

        private void MapControl_Initialized(object sender, EventArgs e)
        {
            // Create the interop host control.
            System.Windows.Forms.Integration.WindowsFormsHost host =
                        new System.Windows.Forms.Integration.WindowsFormsHost();

            axMap1 = new AxMapWinGIS.AxMap();
            host.Child = axMap1;

            new GlobalSettings() { BingApiKey = "AimcXg9FM3tvlLlm3DJlO7kw_8QRFCJI5BkRA0IWJQP-Y5wtZJGJw81C-YuTcMMF" };

            string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            //new GlobalSettings().StartLogTileRequests(localData + @"\ResTBDesktop\tiles.txt");

            
            new GlobalSettings().AllowLayersWithoutProjections = true;
            new GlobalSettings().AllowLayersWithIncompleteReprojection = true;
            new GlobalSettings().ReprojectLayersOnAdding = true;

            this.ResTBMap.Children.Add(host);
            _tools = new MapControlTools(axMap1);



        }


    }
}
