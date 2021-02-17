using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB.DB.Models
{
    /// <summary>
    /// Default resilience weights for resilience factors. Divided by beforeAction.
    /// </summary>
    public class ResilienceWeight
    {
        public int ID { get; set; }

        public ResilienceFactor ResilienceFactor { get; set; }

        public NatHazard NatHazard { get; set; }
        
        public double Weight { get; set; }

        public bool? BeforeAction { get; set; }

        public virtual List<ResilienceValues> ResilienceValues { get; set; }


    }
}