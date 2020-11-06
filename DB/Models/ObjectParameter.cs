using ResTB.Translation;
using ResTB.Translation.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ResTB.DB.Models
{
    public class Objectparameter : ICloneable
    {
        [Browsable(false)]
        [ReadOnly(true)]
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Browsable(false)]
        public ObjectClass ObjectClass { get; set; }

        [Browsable(false)]
        public FeatureType FeatureType { get; set; }

        [LocalizedDisplayName(nameof(Resources.OP_Name), typeof(Resources))]
        [Display(Order = 1)]
        public string Name { get; set; } = string.Empty;
        [LocalizedDisplayName(nameof(Resources.Description), typeof(Resources))]
        [Display(Order = 2)]
        public string Description { get; set; } = string.Empty;
        [LocalizedDisplayName(nameof(Resources.OP_ValuePerUnit), typeof(Resources))]
        [Display(Order = 3)]
        public int Value { get; set; }
        [LocalizedDisplayName(nameof(Resources.OP_Unity), typeof(Resources))]
        [Display(Order = 4)]
        [ReadOnly(true)]
        public string Unity { get; set; } = string.Empty;
        [LocalizedDisplayName(nameof(Resources.OP_ValueChange), typeof(Resources))]
        [Display(Order = 5)]
        public string ChangeValue { get; set; } = string.Empty;
        [LocalizedDisplayName(nameof(Resources.OP_Floors), typeof(Resources))]
        [Display(Order = 6)]
        public int Floors { get; set; }
        [LocalizedDisplayName(nameof(Resources.OP_PersonFloor), typeof(Resources))]
        [Display(Order = 7)]
        public int Personcount { get; set; }
        [LocalizedDisplayName(nameof(Resources.OP_PersonChange), typeof(Resources))]
        [Display(Order = 8)]
        public string ChangePersonCount { get; set; } = string.Empty;
        [LocalizedDisplayName(nameof(Resources.OP_Presence), typeof(Resources))]
        [Display(Order = 9)]
        public double Presence { get; set; }
        [LocalizedDisplayName(nameof(Resources.OP_Vehicles), typeof(Resources))]
        [Display(Order = 10)]
        public int NumberOfVehicles { get; set; }
        [LocalizedDisplayName(nameof(Resources.OP_VehicleSpeed), typeof(Resources))]
        [Display(Order = 11)]
        public int Velocity { get; set; }
        [LocalizedDisplayName(nameof(Resources.OP_Staffs), typeof(Resources))]
        [Display(Order = 12)]
        public int Staff { get; set; }

        [Browsable(false)]
        [ReadOnly(true)]
        public bool IsStandard { get; set; }

        [Browsable(false)]
        public virtual List<ObjectparameterPerProcess> ObjectparameterPerProcesses { get; set; }

        [Browsable(false)]
        public virtual Objectparameter MotherOtbjectparameter { get; set; }

        //[Browsable(false)]
        public virtual List<ObjectparameterHasProperties> HasProperties { get; set; }

        [Browsable(false)]
        public virtual List<ResilienceFactor> ResilienceFactors { get; set; }

        /// <summary>
        /// Used for editing resilience values
        /// </summary>
        [NotMapped]
        [Browsable(false)]
        public List<ResilienceValues> ResilienceValuesMerged { get; set; } = new List<ResilienceValues>();

        [NotMapped]
        [Browsable(false)]
        public ObservableCollection<ResilienceValues> ResilienceValuesMergedBefore =>
            new ObservableCollection<ResilienceValues>(ResilienceValuesMerged.Where(rv => rv.ResilienceWeight.BeforeAction == true));

        [NotMapped]
        [Browsable(false)]
        public ObservableCollection<ResilienceValues> ResilienceValuesMergedAfter =>
            new ObservableCollection<ResilienceValues>(ResilienceValuesMerged.Where(rv => rv.ResilienceWeight.BeforeAction == false));


        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// Clone for not standard mapped objects
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static Objectparameter CloneObject(Objectparameter o)
        {
            Objectparameter clone = new Objectparameter();
            if (o.Name != null) clone.Name = (string)o.Name.Clone();
            if (o.Description != null) clone.Description = (string)o.Description.Clone();
            clone.Value = o.Value;
            if (o.ChangeValue != null) clone.ChangeValue = (string)o.ChangeValue.Clone();
            if (o.Unity != null) clone.Unity = (string)o.Unity.Clone();
            clone.Floors = o.Floors;
            clone.Personcount = o.Personcount;
            if (o.ChangePersonCount != null) clone.ChangePersonCount = (string)o.ChangePersonCount.Clone();
            clone.Presence = o.Presence;
            clone.NumberOfVehicles = o.NumberOfVehicles;
            clone.Velocity = o.Velocity;
            clone.MotherOtbjectparameter = o;
            clone.IsStandard = false;
            clone.ObjectClass = o.ObjectClass;
            clone.FeatureType = o.FeatureType;
            clone.Staff = o.Staff;

            return clone;
        }
    }
}
