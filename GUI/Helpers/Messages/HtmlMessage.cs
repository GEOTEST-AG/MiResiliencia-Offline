using GalaSoft.MvvmLight.Messaging;
using System;

namespace ResTB.GUI.Helpers.Messages
{
    public class HtmlMessage : MessageBase
    {
        public string HtmlString { get; set; } = String.Empty;
        public string Url { get; set; } = String.Empty;

        public string Message { get; set; }
    }
}
