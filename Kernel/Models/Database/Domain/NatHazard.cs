using System;
using System.Text;
using System.Collections.Generic;


namespace ResTB_API.Models.Database.Domain  {
    
    public class NatHazard {
        public virtual int ID { get; set; }        
        public virtual string Name { get; set; }
        //public virtual List<PrA> PrAs { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
