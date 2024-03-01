using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
        public string Description_EN { get; set; }
        public string Description_ES { get; set; }

        /// <summary>
        /// return period in years
        /// </summary>
        public int Value { get; set; }
        public List<PrA> PrAs { get; set; }

        public int CompareTo(object obj)
        {
            return Description.CompareTo(((IKClasses)obj).Description);
        }

        public override string ToString()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            if (currentCulture.TwoLetterISOLanguageName.ToLower() == "en") return $"{Description_EN}";
            else if (currentCulture.TwoLetterISOLanguageName.ToLower() == "es") return $"{Description_ES}";
            else return $"{Description}";
        }
    }
}
