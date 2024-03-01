using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{
    //Natural hazard, language and object parameter dependent!
    public class NatHazard : IComparable
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Name_EN { get; set; }
        public string Name_ES { get; set; }

        public List<PrA> PrAs { get; set; }
        public List<Intensity> Intensities { get; set; }
        public List<ObjectparameterPerProcess> ObjectparameterPerProcesses { get; set; }

        public int CompareTo(object obj)
        {
            return Name.CompareTo(((NatHazard)obj).Name);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(NatHazard))
                return ID.Equals(((NatHazard)obj).ID);
            else
                return false;
        }

        public override string ToString()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            if (currentCulture.TwoLetterISOLanguageName.ToLower() == "en") return $"{Name_EN}";
            else if (currentCulture.TwoLetterISOLanguageName.ToLower() == "es") return $"{Name_ES}";
            else return $"{Name}";
        }
    }
}
