using ResTB_API.Helpers;
using ResTB_API.Models.Database.Domain;
using ResTB_API.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ResTB_API.Models
{

    public class ScenarioResult
    {
        //[DisplayName("Natural Hazard")]
        [LocalizedDisplayName(nameof(ResResult.PR_NatHazard), typeof(ResResult))]
        public NatHazard NatHazard { get; set; }
        [TableIgnore]
        public bool BeforeAction { get; set; }
        [TableIgnore]
        public string BeforeActionString => BeforeAction ? ResResult.PR_beforeActionString : ResResult.PR_afterActionString;
        //[DisplayName("Return Period")]
        [LocalizedDisplayName(nameof(ResResult.SR_ReturnPeriod), typeof(ResResult))]
        public IKClasses IkClass { get; set; }
        [TableIgnore]
        public List<DamageExtent> DamageExtents { get; set; } = new List<DamageExtent>();
        //[DisplayName("Person Damage")]
        [LocalizedDisplayName(nameof(ResResult.PR_DamageExtentPerson), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public double DamageExtentPerson => DamageExtents.Sum(de => de.PersonDamage);
        //[DisplayName("Deaths")]
        [LocalizedDisplayName(nameof(ResResult.PR_DamageExtentDeaths), typeof(ResResult))]
        public double DamageExtentDeaths => DamageExtents.Sum(de => de.Deaths);
        //[DisplayName("Property Damage")]
        [LocalizedDisplayName(nameof(ResResult.PR_DamageExtentProperty), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public double DamageExtentProperty => DamageExtents.Sum(de => de.PropertyDamage);
        /// <summary>
        /// Total resilient indirect damage
        /// </summary>
        //[DisplayName("Resilient Indirect Damage")]
        [LocalizedDisplayName(nameof(ResResult.SR_DamageExtentIndirectResilient), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public double DamageExtentIndirect => DamageExtents.Sum(de => de.ResilientIndirectDamage);
        //[DisplayName("Total Damage")]
        [LocalizedDisplayName(nameof(ResResult.PR_DamageExtentTotal), typeof(ResResult))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public double DamageExtentTotal => DamageExtentPerson + DamageExtentProperty + DamageExtentIndirect;

        #region Detailed Damage Extents
        //public double BuildingPersonDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 1).Sum(de => de.PersonDamage);
        //public double BuildingPropertyDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 1).Sum(de => de.PropertyDamage);
        //public double BuildingResilientDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 1).Sum(de => de.ResilientIndirectDamage);

        //public double SpecialObjPersonDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 2).Sum(de => de.PersonDamage);
        //public double SpecialObjPropertyDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 2).Sum(de => de.PropertyDamage);
        //public double SpecialObjResilientDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 2).Sum(de => de.ResilientIndirectDamage);

        //public double InfrastructurePersonDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 3).Sum(de => de.PersonDamage);
        //public double InfrastructurePropertyDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 3).Sum(de => de.PropertyDamage);
        //public double InfrastructureResilientDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 3).Sum(de => de.ResilientIndirectDamage);

        ////public double AgriculturePersonDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 4).Sum(de=>de.PersonDamage);
        //public double AgriculturePropertyDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 4).Sum(de => de.PropertyDamage);
        //public double AgricultureResilientDamage => DamageExtents.Where(de => de.MappedObject.Objectparameter.ID == 4).Sum(de => de.ResilientIndirectDamage);
        #endregion  

        public int getID()
        {
            int id = 0;

            id += Convert.ToInt32(BeforeAction) * 1;
            id += NatHazard != null ? NatHazard.ID * 10 : 0;
            id += IkClass != null ? IkClass.ID * 100 : 0;

            return id;
        }
    }
}