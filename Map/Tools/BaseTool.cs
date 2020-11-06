using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Tools
{
    public class BaseTool
    {
        public AxMapWinGIS.AxMap AxMap { get; set; }
        public MapControlTools MapControlTools { get; set; }

        public BaseTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTools)
        {
            this.AxMap = axMap;
            this.MapControlTools = mapControlTools;
        }

        /// <summary>
        /// On_Error called when an Error occurred
        /// </summary>
        public void On_Error(Events.MapControl_Error e) 
        {
            MapControlTools.On_Error(e);
        }

        /// <summary>
        /// On_LayerChange when a Layer is added, removed oder changed
        /// </summary>
        public void On_LayerChange(Events.MapControl_LayerChange e) 
        {
            MapControlTools.On_LayerChange(e);
        }

        /// <summary>
        /// On_EditingStateChange when a starting editing, stopping, ...
        /// </summary>
        public void On_EditingStateChange(Events.MapControl_EditingStateChange e) 
        {
            MapControlTools.On_EditingStateChange(e);
        }

        /// <summary>
        /// On_SelectingStateChange when a starting selecting, stopping, ...
        /// </summary>
        public void On_SelectingStateChange(Events.MapControl_SelectingStateChange e)
        {
            MapControlTools.On_SelectingStateChange(e);
        }
    }
}
