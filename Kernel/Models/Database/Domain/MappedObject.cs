using System;
using System.Text;
using System.Collections.Generic;
using NpgsqlTypes;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using ResTB_API.Helpers;
using ResTB_API.Controllers;
using System.Linq;

namespace ResTB_API.Models.Database.Domain
{

    public class MappedObject : ICloneable
    {
        public virtual int ID { get; set; }
        public virtual Objectparameter Objectparameter { get; set; }
        public virtual Project Project { get; set; }
        public virtual IGeometry point { get; set; }
        public virtual IGeometry line { get; set; }
        public virtual IGeometry polygon { get; set; }
        public virtual Objectparameter FreeFillParameter { get; set; }
        public virtual IList<ResilienceValues> ResilienceValues { get; set; }

        //----------------------------------------------------------
        //not in DB
        public virtual bool IsClipped { get; set; } = false;
        public virtual Intensity Intensity { get; set; } = null;
        //public virtual double ResilienceFactor => DamagePotentialController.computeResilienceFactor(ResilienceValues?.ToList());

        public override string ToString()
        {
            return $"{ID} - " +
                $"{Objectparameter?.ObjectClass?.Name ?? "ERROR"} - " +
                $"{Objectparameter?.Name ?? "ERROR"} "; 
            //+
            //    $"({geometry?.GeometryType ?? "ERROR"})";
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}


