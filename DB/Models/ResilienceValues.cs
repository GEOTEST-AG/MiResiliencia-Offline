using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ResTB.DB.Models
{
    /// <summary>
    /// Resilience values for a mapped object
    /// </summary>
    public class ResilienceValues : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ID { get; set; }
        public MappedObject MappedObject { get; set; }

        public ResilienceWeight ResilienceWeight { get; set; }

        [Range(0, 1)]
        public double OverwrittenWeight { get; set; }

        private double _value;
        [Range(0, 1)]
        //[AlsoNotifyFor("OverwrittenWeight")]
        public double Value
        {
            get => _value; 
            set
            {
                _value = value;
                //if (_value > 0 && OverwrittenWeight == 0) //activate resilience value by setting the weight = 1
                //{
                //    OverwrittenWeight = 1;
                    
                //}
            }
        }
    }
}