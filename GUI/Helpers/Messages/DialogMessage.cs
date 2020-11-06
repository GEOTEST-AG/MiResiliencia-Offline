using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows;

namespace ResTB.GUI.Helpers.Messages
{
    public class DialogMessage : GenericMessage<string>
    {
        /// <summary>
        /// Initializes a new instance of the DialogMessage class.
        /// </summary>
        /// <param name="content">The text displayed by the message box.</param>
        /// <param name="callback">A callback method that should be executed to deliver the result
        /// of the message box to the object that sent the message.</param>
        public DialogMessage(
            string content,
            Action<MessageBoxResult> callback)
            : base(content)
        {
            Callback = callback;
        }

        /// <summary>
        /// Initializes a new instance of the DialogMessage class.
        /// </summary>
        /// <param name="sender">The message's original sender.</param>
        /// <param name="content">The text displayed by the message box.</param>
        /// <param name="callback">A callback method that should be executed to deliver the result
        /// of the message box to the object that sent the message.</param>
        public DialogMessage(
            object sender,
            string caption,
            string content,
            Action<MessageBoxResult> callback,
            MessageBoxResult defaultResult = MessageBoxResult.No
            )
            : base(sender, content)
        {
            Caption = caption;
            Callback = callback;
            DefaultResult = defaultResult;
        }

        ///// <summary>
        ///// Initializes a new instance of the DialogMessage class.
        ///// </summary>
        ///// <param name="sender">The message's original sender.</param>
        ///// <param name="target">The message's intended target. This parameter can be used
        ///// to give an indication as to whom the message was intended for. Of course
        ///// this is only an indication, amd may be null.</param>
        ///// <param name="content">The text displayed by the message box.</param>
        ///// <param name="callback">A callback method that should be executed to deliver the result
        ///// of the message box to the object that sent the message.</param>
        //public DialogMessage(
        //    object sender,
        //    object target,
        //    string content,
        //    Action<MessageBoxResult> callback)
        //    : base(sender, target, content)
        //{
        //    Callback = callback;
        //}

        /// <summary>
        /// Displayed buttons by the message box.
        /// </summary>
        public MessageBoxButton Button { get; set; } = MessageBoxButton.YesNo;

        /// <summary>
        /// Gets a callback method that should be executed to deliver the result
        /// of the message box to the object that sent the message.
        /// </summary>
        public Action<MessageBoxResult> Callback { get; private set; }

        /// <summary>
        /// Caption for the message box.
        /// </summary>
        public string Caption { get; set; } = "Caption";

        /// <summary>
        /// Default result of MessageBox, default = No
        /// </summary>
        public MessageBoxResult DefaultResult { get; set; } = MessageBoxResult.No;

        /// <summary>
        /// MessageBox Icon, default = Question
        /// </summary>
        public MessageBoxImage Icon { get; set; } = MessageBoxImage.Question;

        /// <summary>
        /// MessageBox Options, default = None
        /// </summary>
        public MessageBoxOptions Options { get; set; } = MessageBoxOptions.None;

        ///// <summary>
        ///// Utility method, checks if the <see cref="Callback" /> property is
        ///// null, and if it is not null, executes it.
        ///// </summary>
        ///// <param name="result">The result that must be passed
        ///// to the dialog message caller.</param>
        //public void ProcessCallback(MessageBoxResult result)
        //{
        //    if (Callback != null)
        //    {
        //        Callback(result);
        //    }
        //}
    }
}
