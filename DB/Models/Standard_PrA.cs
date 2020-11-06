using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ResTB.DB.Models
{
    public class Standard_PrA
    {
        [Key, Column(Order = 0)]
        public int NatHazardId { get; set; }
        [Key, Column(Order = 1)]
        public int IKClassesId { get; set; }
        public NatHazard NatHazard { get; set; }
        public IKClasses IKClasses { get; set; }
        public double Value { get; set; }
    }
}