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
    /// <summary>
    /// Types of different views to set the recipient of a message
    /// </summary>
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
            InitializeComponent();

            //handle mouse clicks
            this.PreviewMouseDown += new MouseButtonEventHandler(MainWindow_PreviewMouseDown);

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
                        //message.Title sets the message box image
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

        /// <summary>
        /// set the map control in the view model
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //TODO: is there a nicer way to send the Control to the ViewModel? --> Could be done via map message
                ((MainViewModel)(this.DataContext)).MapControl = Karte;                                             //set map control

                var message = new MapMessage() { MessageType = MapMessageType.Initialized, Boolean = true };        //let everyone know that the map control is ready
                Messenger.Default.Send(message);

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Avoid opening the context menu on ribbons
        /// </summary>
        private void RibbonMain_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Handling the resilience slider changes
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                var slider = (Slider)sender;

                if (slider.IsFocused || slider.IsKeyboardFocused || slider.IsMouseOver) //only direct mouse or keyboard hits evaluated
                {
                    var weight = ((ResilienceValues)(slider).DataContext).OverwrittenWeight;

                    if (e.NewValue > 0 && weight == 0)  //activate resilience value by setting the weight = 1
                    {
                        ((ResilienceValues)(slider).DataContext).OverwrittenWeight = 1;
                    }
                }
            }
            e.Handled = true;
        }
    }
}
