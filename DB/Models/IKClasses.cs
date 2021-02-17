using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{
    /// <summary>
    /// Return period for risk calculation
    /// </summary>
    public class IKClasses : IComparable
    {
        public int ID { get; set; }
        /// <summary>
        /// language dependent description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// return period in years
        /// </summary>
        public int Value { get; set; }
        public List<PrA> PrAs { get; set; }

        public int CompareTo(object obj)
        {
            return Description.CompareTo(((IKClasses)obj).Description);
        }
    }
}
