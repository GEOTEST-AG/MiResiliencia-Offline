using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{

    public enum IntensityDegree
    {
        /// <summary>
        /// high intensity
        /// </summary>
        alta = 0,
        /// <summary>
        /// medium intensity
        /// </summary>
        media = 1,  
        /// <summary>
        /// low intensity
        /// </summary>
        baja = 2 ,
        /// <summary>
        /// no intensity
        /// </summary>
        zero = 3,
    }

    public class Intensity
    {
        public int ID { get; set; }
        public Project Project { get; set; }
        public NatHazard NatHazard { get; set; }
        public IKClasses IKClasses { get; set; }
        public bool BeforeAction { get; set; }
        public IntensityDegree IntensityDegree { get; set; }
        public virtual List<DamageExtent> DamageExtents { get; set; }



    }
}
