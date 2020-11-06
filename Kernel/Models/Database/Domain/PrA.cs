using ResTB_API.Helpers;
using ResTB_API.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models.Database.Domain
{
    public class PrA
    {
        [LocalizedDisplayName(nameof(ResModel.PA_Hazard), typeof(ResModel))]
        public virtual NatHazard NatHazard { get; set; }
        [LocalizedDisplayName(nameof(ResModel.PA_ReturnPeriod), typeof(ResModel))]
        public virtual IKClasses IKClass { get; set; }
        [LocalizedDisplayName(nameof(ResModel.PA_Value), typeof(ResModel))]
        public virtual double Value { get; set; }
        [TableIgnore]
        public virtual Project Project { get; set; }

        //composite-id class must override Equals():
        public override bool Equals(object obj)
        {
            var other = obj as PrA;

            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return this.NatHazard.ID == other.NatHazard.ID &&
                this.IKClass.ID == other.IKClass.ID && this.Project.Id == other.Project.Id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = GetType().GetHashCode();
                hash = (hash * 31) ^ NatHazard.GetHashCode();
                hash = (hash * 31) ^ IKClass.GetHashCode();
                hash = (hash * 31) ^ Project.GetHashCode();

                return hash;
            }
        }

        public override string ToString()
        {
            return $"ProjectID {Project.Id}, {NatHazard.Name}, {IKClass.Value}a, {Value}";
        }
    }
}