using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{
    /// <summary>
    /// a mapped object with parameters, damage extents, and resilience values
    /// </summary>
    public class MappedObject
    {
        [ReadOnly(true)]
        public int ID { get; set; }
        [ReadOnly(true)]
        public Objectparameter Objectparameter { get; set; }
        [ReadOnly(true)]
        public Objectparameter FreeFillParameter { get; set; }

        [Browsable(false)]
        public virtual List<ResilienceValues> ResilienceValues { get; set; }
        [Browsable(false)]
        public virtual List<DamageExtent> DamageExtents { get; set; }
        [ReadOnly(true)]
        public Project Project { get; set; }

        //[ReadOnly(true)]
        //[NotMapped]
        //public double lat { get; set; }
        //[ReadOnly(true)]
        //[NotMapped]
        //public double lon { get; set; }

    }
}
