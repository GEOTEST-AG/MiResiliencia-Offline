using System;
using System.Text;
using System.Collections.Generic;
using ResTB_API.Resources;

namespace ResTB_API.Models.Database.Domain
{

    public class IKClasses
    {
        public virtual int ID { get; set; }
        public virtual string Description { get; set; }
        public virtual int Value { get; set; }


        public override string ToString()
        {
            return $"{Value} {ResModel.IK_Years}";
        }
    }
}
