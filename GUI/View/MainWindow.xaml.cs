using Fluent;
using GalaSoft.MvvmLight.Messaging;
using ResTB.DB.Models;
using ResTB.GUI.Helpers.Messages;
using ResTB.GUI.ViewModel;
using SplashScreen;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ResTB.GUI.View
{

    public enum WindowType
    {
        MainWindow,
        NewProjectWindow,
        OpenProjectWindow
    }
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            //InitializeMap();
            InitializeComponent();

            this.PreviewMouseDown += new MouseButtonEventHandler(MainWindow_PreviewMouseDown);


            //TESTING Webbrowser
            //string localData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //if (!System.IO.Directory.Exists(localData + "\\ResTBDesktop"))
            //    System.IO.Directory.CreateDirectory(localData + "\\ResTBDesktop");
            //string htmlPath = localData + "\\ResTBDesktop\\result.html";
            //myWebBrowser.Visibility = Visibility.Visible;
            //myWebBrowser.Navigate(htmlPath);

            Closing += (s, e) => ViewModelLocator.Cleanup();    

            // register MapMessage to change AxMap properties
            Messenger.Default.Register<MapMessage>(
                this,
                message =>
                {
                    switch (message.MessageType)
                    {
                        case MapMessageType.Default:
                            throw new NotImplementedException();
                            break;
                        case MapMessageType.Message:
                            throw new NotImplementedException();
                            break;
                        case MapMessageType.KnownExtent:
                            Karte.Tools.AxMap.KnownExtents = message.KnownExtent;           //Set map extent
                            break;
                        case MapMessageType.TileProvider:
                            //Karte.Tools.AxMap.TileProvider = message.TileProvider;
                            Karte.Tools.AxMap.Tiles.ProviderId = message.TileProviderId;    //Set tile provider
                            Karte.Tools.Redraw(false);
                            break;
                        case MapMessageType.CursorMode:                                     //Set cursor mode
                            Karte.Tools.AxMap.CursorMode = message.CursorMode;
                            break;
                        default:
                            break;
                    }
                }
                );

            // register MessageBoxMessage for showing modal / non-modal message boxes
            Messenger.Default.Register<MessageBoxMessage>(
                this,
                message =>
                {
                    // If showing an error message, make sure to close the splash screen immediately, using SplashScreenAdapter.CloseSplashScreen(),
                    // before you show a message box.
                    SplashScreenAdapter.CloseSplashScreen();

                    if (message.IsModal)
                    {
                        if (message.Title.ToLower().Contains("ERROR".ToLower()))
                            MessageBox.Show(message.Message, message.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        else if (message.Title.ToLower().Contains("warning".ToLower()))
                            MessageBox.Show(message.Message, message.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                        else if (message.Title.ToLower().Contains("INFO".ToLower()))
                            MessageBox.Show(message.Message, message.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        else
                            MessageBox.Show(message.Message, message.Title);
                    }
                    else
                        Task.Run(() =>
                        {
                            MessageBox.Show(message.Message, message.Title);
                        });

                }
            );

            // register DialogMessage for showing dialog boxes in MainWindow
            Messenger.Default.Register<DialogMessage>(
              this, WindowType.MainWindow,
              message =>
              {
                  if (message == null)
                  {
                      return;
                  }

                  var result = MessageBox.Show(
                      this,
                      message.Content,
                      message.Caption,
                      message.Button,
                      message.Icon,             //Default: Question Icon
                      message.DefaultResult,
                      message.Options);

                  if (message.Callback != null)
                  {
                      message.Callback(result); // execute callback: send result to MainViewModel
                  }
              });
        
        }

        /// <summary>
        /// Avoid clicking around while busy
        /// </summary>
        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (((MainViewModel)(this.DataContext)).Cursor == Cursors.Wait)
            {
                e.Handled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //TODO: is there a nicer way to send the Control to the ViewModel?
                ((MainViewModel)(this.DataContext)).MapControl = Karte;

                var message = new MapMessage() { MessageType = MapMessageType.Initialized, Boolean = true };
                Messenger.Default.Send(message);

            }
            catch (Exception)
            {

                throw;
            }

            //var StartWindow = new StartWindow();
            //StartWindow.ShowDialog();
        }

        /// <summary>
        /// Avoid opening the context menu on ribbons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonMain_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                var slider = (Slider)sender;

                if (slider.IsFocused || slider.IsKeyboardFocused || slider.IsMouseOver)
                {
                    var weight = ((ResilienceValues)(slider).DataContext).OverwrittenWeight;

                    if (e.NewValue > 0 && weight == 0) //activate resilience value by setting the weight = 1
                    {
                        ((ResilienceValues)(slider).DataContext).OverwrittenWeight = 1;
                    }
                }
            }
            e.Handled = true;
        }
    }
}
