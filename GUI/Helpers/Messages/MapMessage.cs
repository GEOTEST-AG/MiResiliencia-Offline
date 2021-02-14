using GalaSoft.MvvmLight.Messaging;
using MapWinGIS;
using ResTB.Map;
using System.Collections.Generic;
using System.Text;

namespace ResTB.GUI.Helpers.Messages
{
    /// <summary>
    /// What the map message should deliever
    /// </summary>
    public enum MapMessageType
    {
        Default = 0,
        /// <summary>
        /// send message (debug)
        /// </summary>
        Message,
        /// <summary>
        /// set map extent
        /// </summary>
        KnownExtent,
        /// <summary>
        /// set tile provider
        /// </summary>
        TileProvider,
        /// <summary>
        /// set cursor mode
        /// </summary>
        CursorMode,
        /// <summary>
        /// map control is ready
        /// </summary>
        Initialized
    }

    /// <summary>
    /// Message to change map control (AxMap) properties
    /// </summary>
    public class MapMessage : MessageBase
    {
        public MapMessageType MessageType { get; set; } = MapMessageType.Default;

        public string Message { get; set; }
        public tkKnownExtents KnownExtent { get; set; }
        public int TileProviderId { get; set; }
        public tkCursorMode CursorMode { get; set; }
        /// <summary>
        /// not in use so far
        /// </summary>
        public bool Boolean { get; set; }   

    }
}
