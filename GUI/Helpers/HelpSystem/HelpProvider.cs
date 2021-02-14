using ResTB.GUI.Helpers.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace ResTB.GUI.Helpers.HelpSystem
{
    //Source:
    //https://stackoverflow.com/questions/5116465/integrating-help-in-a-wpf-application

    /// <summary>
    /// Access to chm help file from WPF
    /// </summary>
    public class HelpProvider
    {
        #region Fields

        /// <summary>
        /// Help topic dependency property. 
        /// </summary>
        /// <remarks>This property can be attached to an object such as a form or a textbox, and 
        /// can be retrieved when the user presses F1 and used to display context sensitive help.</remarks>
        public static readonly DependencyProperty HelpTopicProperty =
            DependencyProperty.RegisterAttached("HelpString", typeof(string), typeof(HelpProvider));

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Static constructor that adds a command binding to Application.Help, binding it to 
        /// the CanExecute and Executed methods of this class. 
        /// </summary>
        /// <remarks>With this in place, when the user presses F1 our help will be invoked.</remarks>
        static HelpProvider()
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(FrameworkElement),
                new CommandBinding(
                    ApplicationCommands.Help,
                    new ExecutedRoutedEventHandler(ShowHelpExecuted),
                    new CanExecuteRoutedEventHandler(ShowHelpCanExecute)));
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Getter for <see cref="HelpTopicProperty"/>. Get a help topic that's attached to an object. 
        /// </summary>
        /// <param name="obj">The object that the help topic is attached to.</param>
        /// <returns>The help topic.</returns>
        public static string GetHelpTopic(DependencyObject obj)
        {
            return (string)obj.GetValue(HelpTopicProperty);
        }

        /// <summary>
        /// Setter for <see cref="HelpTopicProperty"/>. Attach a help topic value to an object. 
        /// </summary>
        /// <param name="obj">The object to which to attach the help topic.</param>
        /// <param name="value">The value of the help topic.</param>
        public static void SetHelpTopic(DependencyObject obj, string value)
        {
            obj.SetValue(HelpTopicProperty, value);
        }

        /// <summary>
        /// Show help table of contents. 
        /// </summary>
        public static void ShowHelpTableOfContents()
        {
#if DEBUG
            MessageBoxMessage.Send("ShowHelpTableOfContents", "Klicked", true);
#endif 

            var culture = Thread.CurrentThread.CurrentUICulture;
            switch (culture.TwoLetterISOLanguageName)
            {
                case "en":
                    if (!File.Exists("en/MiResiliencia Desktop.chm"))
                        goto default;
                    System.Windows.Forms.Help.ShowHelp(null, "en/MiResiliencia Desktop.chm", HelpNavigator.TableOfContents);
                    break;
                case "de":
                    if (!File.Exists("de/MiResiliencia Desktop.chm"))
                        goto default;
                    System.Windows.Forms.Help.ShowHelp(null, "de/MiResiliencia Desktop.chm", HelpNavigator.TableOfContents);
                    break;
                case "es":
                default:
                    System.Windows.Forms.Help.ShowHelp(null, "es/MiResiliencia Desktop.chm", HelpNavigator.TableOfContents);
                    break;
            }

        }

        /// <summary>
        /// Show a help topic in the online CHM style help. 
        /// </summary>
        /// <param name="helpTopic">The help topic to show. This must match exactly with the name 
        /// of one of the help topic's .htm files, without the .htm extention and with spaces instead of underscores
        /// in the name. For instance, to display the help topic "This_is_my_topic.htm", pass the string "This is my topic".</param>
        /// <remarks>You can also pass in the help topic with the underscore replacement already done. You can also 
        /// add the .htm extension. 
        /// Certain characters other than spaces are replaced by underscores in RoboHelp help topic names. 
        /// This method does not yet account for all those replacements, so if you really need to find a help topic
        /// with one or more of those characters, do the underscore replacement before passing the topic.</remarks>
        public static void ShowHelpTopic(string helpTopic)
        {
            // Strip off trailing period.
            if (helpTopic.IndexOf(".") == helpTopic.Length - 1)
                helpTopic = helpTopic.Substring(0, helpTopic.Length - 1);

            helpTopic = helpTopic.Replace(" ", "_").Replace("\\", "_").Replace("/", "_").Replace(":", "_").Replace("*", "_").Replace("?", "_").Replace("\"", "_").Replace(">", "_").Replace("<", "_").Replace("|", "_") + (helpTopic.IndexOf(".htm") == -1 ? ".htm" : "");

#if DEBUG
            MessageBoxMessage.Send("ShowHelpTopic", "Klicked on " + helpTopic, true);
#endif 

            var culture = Thread.CurrentThread.CurrentUICulture;
            switch (culture.TwoLetterISOLanguageName)
            {
                case "en":
                    if (!File.Exists("en/MiResiliencia Desktop.chm"))
                        goto default;
                    Help.ShowHelp(null, "en/MiResiliencia Desktop.chm", HelpNavigator.Topic, helpTopic);
                    break;
                case "de":
                    if (!File.Exists("de/MiResiliencia Desktop.chm"))
                        goto default;
                    Help.ShowHelp(null, "de/MiResiliencia Desktop.chm", HelpNavigator.Topic, helpTopic);
                    break;
                case "es":
                default:
                    Help.ShowHelp(null, "es/MiResiliencia Desktop.chm", HelpNavigator.Topic, helpTopic);
                    break;
            }
        }

        /// <summary>
        /// Whether the F1 help command can execute. 
        /// </summary>
        private static void ShowHelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;

            if (HelpProvider.GetHelpTopic(senderElement) != null)
                e.CanExecute = true;
        }

        /// <summary>
        /// Execute the F1 help command. 
        /// </summary>
        /// <remarks>Calls ShowHelpTopic to show the help topic attached to the framework element that's the 
        /// source of the call.</remarks>
        private static void ShowHelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ShowHelpTopic(HelpProvider.GetHelpTopic(sender as FrameworkElement));
        }

        #endregion Methods
    }
}
