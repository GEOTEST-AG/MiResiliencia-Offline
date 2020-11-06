using SplashScreen;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;

namespace ResTB.GUI.View
{
    /// <summary>
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    [SplashScreen(MinimumVisibilityDuration = 2, FadeoutDuration = 1)] //2,1
    public partial class MySplashScreen : UserControl
    {
        public MySplashScreen()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the file description.
        /// </summary>
        public FileVersionInfo FileVersionInfo { get; } = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);


    }
}
