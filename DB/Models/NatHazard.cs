using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{
    //Natural hazard, language and object parameter dependent!
    public class NatHazard : IComparable
    {
        public int ID { get; set; }
        public string Name { get; set; }

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
            return $"{Name}";
        }
    }
}
