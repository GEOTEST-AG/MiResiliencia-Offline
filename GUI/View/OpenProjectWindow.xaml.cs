using GalaSoft.MvvmLight.Messaging;
using ResTB.GUI.Helpers.Messages;
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
using System.Windows.Shapes;

namespace ResTB.GUI.View
{
    /// <summary>
    /// Interaction logic for NewProjectWindow.xaml
    /// </summary>
    public partial class OpenProjectWindow : Window
    {
        public OpenProjectWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<DialogMessage>(
             this, WindowType.OpenProjectWindow,
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
                     message.Icon,
                     message.DefaultResult,
                     message.Options);

                 if (message.Callback != null)
                 {
                     message.Callback(result); // execute callback: send result to MainViewModel
                  }
             });

        }

        private void OpenProjectWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Messenger.Default.Unregister<DialogMessage>(this);
        }
    }
}
