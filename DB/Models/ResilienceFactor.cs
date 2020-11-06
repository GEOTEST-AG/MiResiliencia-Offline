using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB.DB.Models
{
    public class ResilienceFactor
    {
        public string ID { get; set; }
        public string Preparedness { get; set; }

        public virtual List<ResilienceWeight> ResilienceWeights { get; set; }
        public List<Objectparameter> Objectparameters { get; set; }
    }
    
}