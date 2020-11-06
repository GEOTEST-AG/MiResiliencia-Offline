using System;
using System.Text;
using System.Collections.Generic;
using GeoAPI.Geometries;
using System.ComponentModel;
using ResTB_API.Helpers;
using ResTB_API.Resources;

namespace ResTB_API.Models.Database.Domain
{

    public class DamageExtent
    {
        [LocalizedDisplayName(nameof(ResModel.DE_MappedObject), typeof(ResModel))]
        public virtual MappedObject MappedObject { get; set; }  // FK \__
        [LocalizedDisplayName(nameof(ResModel.DE_Intensity), typeof(ResModel))]
        public virtual Intensity Intensity { get; set; }        // FK /  \>PK 

        [TableIgnore]
        public virtual IGeometry geometry { get; set; }
        //public virtual IGeometry geometry { get; set; }
        //public virtual IGeometry geometry { get; set; }

        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_Log), typeof(ResModel))]
        public virtual string Log { get; set; }
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_Area), typeof(ResModel))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Meter2_F3), typeof(ResFormat))]
        public virtual double Area { get; set; }   //area [m^2]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_Length), typeof(ResModel))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Meter_F3), typeof(ResFormat))]
        public virtual double Length { get; set; } //length [m^1]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_Piece), typeof(ResModel))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_F0), typeof(ResFormat))]
        public virtual double Piece { get; set; }  //piece [1]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_Clipped), typeof(ResModel))]
        public virtual bool Clipped { get; set; } //flag, indicating that geometry has been clipped
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_Part), typeof(ResModel))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_F3), typeof(ResFormat))]
        public virtual double Part { get; set; }
        [LocalizedDisplayName(nameof(ResModel.DE_PersonDamage), typeof(ResModel))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public virtual double PersonDamage { get; set; }
        [LocalizedDisplayName(nameof(ResModel.DE_PropertyDamage), typeof(ResModel))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public virtual double PropertyDamage { get; set; }
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_Deaths), typeof(ResModel))]
        public virtual double Deaths { get; set; }
        [TableIgnore]
        [LocalizedDisplayName(nameof(ResModel.DE_DeathProbability), typeof(ResModel))]
        public virtual double DeathProbability { get; set; }
        [LocalizedDisplayName(nameof(ResModel.DE_IndirectDamage), typeof(ResModel))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public virtual double IndirectDamage { get; set; }       // without resilience
        [LocalizedDisplayName(nameof(ResModel.DE_ResilienceFactor), typeof(ResModel))]
        public virtual double ResilienceFactor { get; set; }     // 0, if no resilience

        //*not in db*
        [LocalizedDisplayName(nameof(ResModel.DE_ResilientIndirectDamage), typeof(ResModel))]
        [LocalizedDisplayFormat(nameof(ResFormat.DF_Currency), typeof(ResFormat))]
        public virtual double ResilientIndirectDamage
        {
            get
            {
                return IndirectDamage * (1.0d - ResilienceFactor);
            }
        }
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_LogResilientIndirectDamage), typeof(ResModel))]
        public virtual string LogResilientIndirectDamage
        {
            get
            {
                return $"ResilientIndirectDamage = IndirectDamage * (1 - ResilienceFactor); \n" +
                       $"ResilientIndirectDamage = {IndirectDamage:C} * (1 - {ResilienceFactor:F3})";
            }
        }
        //end of *not in db*

        //[TableIgnore]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_LogPersonDamage), typeof(ResModel))]
        public virtual string LogPersonDamage { get; set; }
        //[TableIgnore]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_LogPropertyDamage), typeof(ResModel))]
        public virtual string LogPropertyDamage { get; set; }
        //[TableIgnore]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_LogDeaths), typeof(ResModel))]
        public virtual string LogDeaths { get; set; }
        [TableIgnore]
        [LocalizedDisplayName(nameof(ResModel.DE_LogDeathProbability), typeof(ResModel))]
        public virtual string LogDeathProbability { get; set; }
        //[TableIgnore]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_LogIndirectDamage), typeof(ResModel))]
        public virtual string LogIndirectDamage { get; set; }       // before resilience
        //[TableIgnore]
        [ShowInDetail]
        [LocalizedDisplayName(nameof(ResModel.DE_LogResilienceFactor), typeof(ResModel))]
        public virtual string LogResilienceFactor { get; set; }     // 0, if no resilience

        public override bool Equals(object obj)
        {
            var other = obj as DamageExtent;

            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return this.MappedObject.ID == other.MappedObject.ID &&
                this.Intensity.ID == other.Intensity.ID;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = GetType().GetHashCode();
                hash = (hash * 31) ^ MappedObject.GetHashCode();
                hash = (hash * 31) ^ Intensity.GetHashCode();

                return hash;
            }
        }
    }
}
