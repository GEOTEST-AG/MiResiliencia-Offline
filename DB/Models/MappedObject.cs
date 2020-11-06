using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB.Models
{
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

        [ReadOnly(true)]
        [NotMapped]
        public double lat { get; set; }
        [ReadOnly(true)]
        [NotMapped]
        public double lon { get; set; }


        public static string ParseLatitude(double value)
        {
            var direction = value < 0 ? 1 : 0;
            return MappedObject.ParseLatituteOrLongitude(value, direction);
        }

        public static string ParseLongitude(double value)
        {
            var direction = value < 0 ? 1 : 0;
            return MappedObject.ParseLatituteOrLongitude(value, direction);
        }

        //This must be a private method because it requires the caller to ensure
        //that the direction parameter is correct.
        public static string ParseLatituteOrLongitude(double value, int direction)
        {
            value = Math.Abs(value);

            var degrees = Math.Truncate(value);

            value = (value - degrees) * 60;

            var minutes = Math.Truncate(value);
            var seconds = (value - minutes) * 60;

            return degrees + "° " + minutes + "' " + Math.Round(seconds) + "\"";
        }


    }
}
