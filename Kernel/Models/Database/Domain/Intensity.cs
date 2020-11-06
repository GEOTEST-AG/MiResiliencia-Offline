using System;
using System.Text;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace ResTB_API.Models.Database.Domain
{

    public class Intensity : ICloneable
    {
        public virtual int ID { get; set; }
        public virtual bool BeforeAction { get; set; }
        public virtual NatHazard NatHazard { get; set; }
        public virtual Project Project { get; set; }
        public virtual IKClasses IKClasses { get; set; } //return period in years
        public virtual IGeometry geometry { get; set; }

        /// <summary>
        /// 0=high, 1=medium, 2=low
        /// </summary>
        public virtual int IntensityDegree { get; set; }


        Dictionary<int, string> IntensityDegreeDic = new Dictionary<int, string>() {
            { 0, "high" },
            { 1, "medium" },
            { 2, "low" }, 
            { 3, "zero" } 
        };
        
        public override string ToString()
        {
            return $"{ID} - {NatHazard.Name} " +
                   $"{IKClasses.Description}, {IntensityDegreeDic[IntensityDegree]}, " +
                   $"before={BeforeAction}";
            //$"geometryExists={geometry != null}";
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
