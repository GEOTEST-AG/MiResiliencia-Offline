using GalaSoft.MvvmLight.Messaging;
using System;

namespace ResTB.GUI.Helpers.Messages
{
    /// <summary>
    /// asks for a MessageBox
    /// </summary>
    public class MessageBoxMessage : MessageBase
    {
        /// <summary>
        /// Message to show
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// MessageBoxTitle
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// not modal MessageBox will be started in separate task
        /// </summary>
        public bool IsModal { get; private set; }

        public DateTime TimeStamp { get; private set; }

        public MessageBoxMessage(string title, string message, DateTime timeStamp, bool isModal)
        {
            this.Title = title;
            this.Message = message;
            this.TimeStamp = timeStamp;
            this.IsModal = isModal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="isModal">not modal MessageBox will be started in separate task; modal boxes block the rest</param>
        public static void Send(string title, string message, bool isModal = false, object token = null)
        {
            var dateTimeNow = DateTime.Now;
            if (token == null)
            {
                Messenger.Default.Send(new MessageBoxMessage(title, message, dateTimeNow, isModal));
            }
            else
            {
                Messenger.Default.Send(new MessageBoxMessage(title, message, dateTimeNow, isModal), token);
            }
        }

    }



}
