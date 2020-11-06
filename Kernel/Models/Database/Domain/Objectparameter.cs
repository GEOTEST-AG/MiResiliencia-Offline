using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models.Database.Domain
{
    public class Objectparameter : ICloneable
    {
        public virtual int ID { get; set; }
        /// <summary>
        /// IMPORTANT: Persons PER Floor 
        /// </summary>
        public virtual int Personcount { get; set; }
        public virtual string ChangePersonCount { get; set; }
        public virtual string Name { get; set; }
        public virtual Objectparameter MotherObjectparameter { get; set; }

        /// <summary>
        /// 0: point, 1: line, 2: polygon
        /// </summary>
        public virtual int FeatureType { get; set; }
        public virtual int Floors { get; set; }
        /// <summary>
        /// hours per day -> 1h/24h
        /// </summary>
        public virtual double Presence { get; set; }
        public virtual int Velocity { get; set; }
        public virtual bool IsStandard { get; set; }
        public virtual int Value { get; set; }
        public virtual string ChangeValue { get; set; }
        public virtual string Description { get; set; }
        public virtual string Unity { get; set; }
        public virtual ObjectClass ObjectClass { get; set; }
        public virtual int NumberOfVehicles { get; set; }
        /// <summary>
        /// Breaking Change: Used for Loss per Day ($/d) per Mapped Object
        /// </summary>
        public virtual int Staff { get; set; }

        public virtual IList<ObjectparameterPerProcess> ProcessParameters { get; set; }
        public virtual IList<ObjectparameterHasProperties> HasProperties { get; set; }

        public override string ToString()
        {
            return $"{ID} - {Name ?? "no name"} - Class {ObjectClass?.Name ?? "no class"}";
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}