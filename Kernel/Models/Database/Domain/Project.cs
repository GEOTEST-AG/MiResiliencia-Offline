using System;
using System.Text;
using System.Collections.Generic;
using GeoAPI.Geometries;
using System.Linq;

namespace ResTB_API.Models.Database.Domain
{

    public class Project
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Number { get; set; }
        public virtual int CoordinateSystem { get; set; }
        public virtual IGeometry geometry { get; set; } 
        //public virtual Company Company { get; set; }
        public virtual ProjectState ProjectState { get; set; }
        public virtual string Description { get; set; }
        public virtual IList<PrA> PrAs { get; set; }
        public virtual IList<ProtectionMeasure> ProtectionMeasures { get; set; }

        public virtual ProtectionMeasure ProtectionMeasure => ProtectionMeasures?.FirstOrDefault();

        public override string ToString()
        {
            return $"Project {Id} - {Name}";
        }
    }
}
