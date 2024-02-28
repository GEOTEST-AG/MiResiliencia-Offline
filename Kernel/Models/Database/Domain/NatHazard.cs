using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;


namespace ResTB_API.Models.Database.Domain  {
    
    public class NatHazard {
        public virtual int ID { get; set; }        
        public virtual string Name { get; set; }
        //public virtual List<PrA> PrAs { get; set; }

        public virtual string Name_EN { get; set; }
        public virtual string Name_ES { get; set; }

        public override string ToString()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            if (currentCulture.TwoLetterISOLanguageName.ToLower() == "en") return $"{Name_EN}";
            else if (currentCulture.TwoLetterISOLanguageName.ToLower() == "es") return $"{Name_ES}";
            else return $"{Name}";
        }
    }
}
