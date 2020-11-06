using GalaSoft.MvvmLight.Messaging;
using MapWinGIS;
using ResTB.Map;
using System.Collections.Generic;
using System.Text;

namespace ResTB.GUI.Helpers.Messages
{
    public enum MapMessageType
    {
        Default = 0,
        Message,
        KnownExtent,
        TileProvider,
        CursorMode,
        Initialized
    }

    public class MapMessage : MessageBase
    {
        public MapMessageType MessageType { get; set; } = MapMessageType.Default;

        public string Message { get; set; }
        public tkKnownExtents KnownExtent { get; set; }
        public int TileProviderId { get; set; }
        public tkCursorMode CursorMode { get; set; }
        public bool Boolean { get; set; }

    }
}
