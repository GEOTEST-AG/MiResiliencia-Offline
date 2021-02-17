using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{
    /// <summary>
    /// damage extent of a mapped object for a specific intensity
    /// </summary>
    public class DamageExtent
    {
        [Key, Column(Order = 0)]
        public int MappedObjectId { get; set; }
        [Key, Column(Order = 1)]
        public int IntensityId { get; set; }
        public MappedObject MappedObject { get; set; }  // FK \__
        public Intensity Intensity { get; set; }        // FK /  \>PK 

        public double Area { get; set; }    //area [m^2]
        public double Length { get; set; }  //length [m^1]
        public double Piece { get; set; }   //count [1]
        public bool Clipped { get; set; }   //flag, indicating that geometry has been clipped

        public double Part { get; set; }

        public double PersonDamage { get; set; }
        public string LogPersonDamage { get; set; }

        public double PropertyDamage { get; set; }
        public string LogPropertyDamage { get; set; }

        public double Deaths { get; set; }
        public string LogDeaths { get; set; }

        public double DeathProbability { get; set; }
        public string LogDeathProbability { get; set; }

        public double IndirectDamage { get; set; }          // before resilience
        public string LogIndirectDamage { get; set; }       // before resilience

        public double ResilienceFactor { get; set; }        // 0, if no resilience
        public string LogResilienceFactor { get; set; }     // 0, if no resilience

        public string Log { get; set; }
    }
}
