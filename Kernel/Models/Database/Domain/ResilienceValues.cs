using System;
using System.Text;
using System.Collections.Generic;


namespace ResTB_API.Models.Database.Domain
{

    public class ResilienceValues
    {
        public virtual int ID { get; set; }
        public virtual double Value { get; set; }
        public virtual MappedObject MappedObject { get; set; }
        public virtual ResilienceWeight ResilienceWeight { get; set; }
        public virtual double OverwrittenWeight { get; set; }

        //not in db
        public virtual double Weight
        {
            get
            {
                if (OverwrittenWeight >= 0)
                {
                    return OverwrittenWeight;
                }
                else
                {
                    return ResilienceWeight?.Weight ?? 0.0;
                }
            }
        }

        public override string ToString()
        {

            return $"Value: {Value} / Weight: {Weight}";
        }
    }
}
