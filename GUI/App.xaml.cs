using GalaSoft.MvvmLight.Threading;
using SplashScreen;
using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace ResTB.GUI
{
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();  // for mvvm light dispatching

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //Setting language on startup
            try
            {
                CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);  //set to "es-HN" in app.config
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (Exception)
            {
                //use system default language
            }
            finally
            {
                // set WPF language
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name))
                    );
            }
        }

        /// <summary>
        /// Handling unhandled exceptions
        /// </summary>
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
