using System;
using System.Windows;
using ResTB.GUI.Services;
using ResTB.GUI.ViewModel;
using Fluent;
using ResTB.GUI.Model;
using ResTB.GUI.View;
using GalaSoft.MvvmLight.Threading;
using System.Windows.Input;
using SplashScreen;
using System.Threading;
using System.Globalization;
using System.Configuration;
using System.Windows.Markup;

/// <summary>
/// Credits to https://marcominerva.wordpress.com/2020/01/07/using-the-mvvm-pattern-in-wpf-applications-running-on-net-core/
/// </summary>
/// 

namespace ResTB.GUI
{
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();


        }

        protected override void OnStartup(StartupEventArgs e)
        {

            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es");
            //Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

            }
            catch (Exception)
            {
                //use system default language
            }
            finally
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name))
                    );

            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // If showing an error message, make sure to close the splash screen immediately, using SplashScreenAdapter.CloseSplashScreen(),
            // before you show a message box.
            SplashScreenAdapter.CloseSplashScreen();

            var ex = e.Exception;

            string message = "An unhandled error just occurred: \n\n" + ex.Message + "\n" + ex.InnerException?.Message;

            MessageBox.Show(
                message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
            e.Handled = true;

        }
    }

}
