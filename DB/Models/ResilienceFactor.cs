using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace ResTB.DB.Models
{
    /// <summary>
    /// Resilience factor for mapped objects, depending on object parameters. language dependent!
    /// </summary>
    public class ResilienceFactor
    {
        public string ID { get; set; }
        public string Preparedness { get; set; }
        public string Preparedness_EN { get; set; }
        public string Preparedness_ES { get; set; }

        public virtual List<ResilienceWeight> ResilienceWeights { get; set; }
        public List<Objectparameter> Objectparameters { get; set; }

        public override string ToString()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            if (currentCulture.TwoLetterISOLanguageName.ToLower() == "en") return $"{Preparedness_EN}";
            else if (currentCulture.TwoLetterISOLanguageName.ToLower() == "es") return $"{Preparedness_ES}";
            else return $"{Preparedness}";
        }
    }
    
}