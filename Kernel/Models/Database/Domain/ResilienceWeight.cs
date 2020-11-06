using System;
using System.Text;
using System.Collections.Generic;


namespace ResTB_API.Models.Database.Domain  {

    public class ResilienceWeight
    {
        public virtual int ID { get; set; }
        public virtual double Weight { get; set; }
        public virtual NatHazard NatHazard { get; set; }
        public virtual string ResilienceFactor_ID { get; set; }
        public virtual bool BeforeAction { get; set; }
    }
}
