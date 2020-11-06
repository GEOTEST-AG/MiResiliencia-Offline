using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{
    public class IndirectDamage
    {
        public int ID { get; set; }
        public List<Objectparameter> Objectparameters { get; set; }
    }
}
