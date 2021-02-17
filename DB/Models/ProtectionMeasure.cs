using ResTB.Translation;
using ResTB.Translation.Properties;
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
    /// a.k.a. Mitigation Measure. Geometries are stored separately in ProtectionMeasureGeometry table.
    /// </summary>
    public class ProtectionMeasure : ICloneable
    {
        [Key]
        [Browsable(false)]
        public int ID { get; set; }

        [LocalizedDisplayName(nameof(Resources.MM_Costs), typeof(Resources))]
        [Display(Order = 1)]
        public int Costs { get; set; }
        [LocalizedDisplayName(nameof(Resources.MM_LifeSpan), typeof(Resources))]
        [Display(Order = 2)]
        public int LifeSpan { get; set; }
        [Browsable(false)]
        public int OperatingCosts { get; set; }
        [LocalizedDisplayName(nameof(Resources.MM_MaintenanceCosts), typeof(Resources))]
        [Display(Order = 4)]
        public int MaintenanceCosts { get; set; }

        //TODO: fixed value for rate of return
        [Browsable(false)]
        [ReadOnly(true)]
        public double RateOfReturn { get; set; } = 5.0;
        [LocalizedDisplayName(nameof(Resources.Description), typeof(Resources))]
        [Display(Order = 6)]
        public string Description { get; set; }

        [Browsable(false)]
        public double ValueAddedTax { get; set; }

        [Browsable(false)]
        public Project Project { get; set; }

        public object Clone()
        {
            ProtectionMeasure pm = new ProtectionMeasure();
            pm.Costs = Costs;
            pm.LifeSpan = LifeSpan;
            pm.OperatingCosts = OperatingCosts;
            pm.MaintenanceCosts = MaintenanceCosts;
            pm.RateOfReturn = RateOfReturn;
            pm.Description = Description;
            pm.ValueAddedTax = ValueAddedTax;
            return pm;
        }
    }
}