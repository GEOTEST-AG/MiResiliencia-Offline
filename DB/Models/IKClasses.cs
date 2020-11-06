using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{
    public class IKClasses : IComparable
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public List<PrA> PrAs { get; set; }

        public int CompareTo(object obj)
        {
            return Description.CompareTo(((IKClasses)obj).Description);
        }
    }
}
