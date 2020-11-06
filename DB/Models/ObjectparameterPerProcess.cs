using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

namespace ResTB.DB.Models
{
    public class ObjectparameterPerProcess
    {
        public int ID { get; set; }
        public NatHazard NatHazard { get; set; }
        public Objectparameter Objektparameter { get; set; }

        // Vulnerability por intensidad
        public double VulnerabilityLow { get; set; }
        public double VulnerabilityMedium { get; set; }
        public double VulnerabilityHigh { get; set; }

        // Mortalidad por intensidad
        public double MortalityLow { get; set; }
        public double MortalityMedium { get; set; }
        public double MortalityHigh { get; set; }

        // Dano indirecto
        public double Value { get; set; }
        public string Unit { get; set; }

        //Vulnerability of indirect damage
        public double IndirectVulnerabilityLow { get; set; }
        public double IndirectVulnerabilityMedium { get; set; }
        public double IndirectVulnerabilityHigh { get; set; }

        // Duracion de dano indirecto
        public double DurationLow { get; set; }
        public double DurationMedium { get; set; }
        public double DurationHigh { get; set; }

    }
}