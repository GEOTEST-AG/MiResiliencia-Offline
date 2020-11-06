using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models.Database.Domain
{
    public class ObjectparameterPerProcess
    {
        public virtual int ID { get; set; }
        public virtual NatHazard NatHazard { get; set; }
        public virtual Objectparameter Objectparameter { get; set; }

        //Direct damage
        public virtual double VulnerabilityLow { get; set; }
        public virtual double VulnerabilityMedium { get; set; }
        public virtual double VulnerabilityHigh { get; set; }

        public virtual double MortalityLow { get; set; }
        public virtual double MortalityMedium { get; set; }
        public virtual double MortalityHigh { get; set; }

        //Indirect damage
        /// <summary>
        /// Costs $/day or $/hectare/day
        /// </summary>
        public virtual double Value { get; set; } 
        public virtual string Unit { get; set; }    //Unit: employee or hectare

        public virtual double DurationLow { get; set; }
        public virtual double DurationMedium { get; set; }
        public virtual double DurationHigh { get; set; }

        // to delete

        //public virtual double IndirectVulnerabilityLow { get; set; }
        //public virtual double IndirectVulnerabilityMedium { get; set; }
        //public virtual double IndirectVulnerabilityHigh { get; set; }

        public override string ToString()
        {
            return $"{ID} - NatHazard: {NatHazard.ID}-{NatHazard.Name}, ObjParameter: {Objectparameter.ID}";
        }
    }
}