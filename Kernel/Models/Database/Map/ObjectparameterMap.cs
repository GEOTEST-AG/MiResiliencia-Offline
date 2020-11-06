using FluentNHibernate.Mapping;
using ResTB_API.Models.Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models.Database.Map
{
    public class ObjectparameterMap : ClassMap<Objectparameter>
    {

        public ObjectparameterMap()
        {
            Schema("public");
            Table("\"Objectparameter\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).Column("\"ID\"");
            Map(x => x.Personcount).Column("\"Personcount\"");
            Map(x => x.Name).Nullable().Column("\"Name\"");
            Map(x => x.FeatureType).Column("\"FeatureType\"");
            Map(x => x.Floors).Column("\"Floors\"");
            Map(x => x.ChangeValue).Nullable().Column("\"ChangeValue\"");
            Map(x => x.Presence).Column("\"Presence\"");
            Map(x => x.Velocity).Column("\"Velocity\"");
            Map(x => x.IsStandard).Column("\"IsStandard\"");
            Map(x => x.Value).Column("\"Value\"");
            Map(x => x.Description).Nullable().Column("\"Description\"");
            Map(x => x.Unity).Nullable().Column("\"Unity\"");
            Map(x => x.ChangePersonCount).Nullable().Column("\"ChangePersonCount\"");
            Map(x => x.NumberOfVehicles).Column("\"NumberOfVehicles\"");
            Map(x => x.Staff).Column("\"Staff\"");
            References<Objectparameter>(x => x.MotherObjectparameter).Column("\"MotherOtbjectparameter_ID\"").Nullable();
            References<ObjectClass>(x => x.ObjectClass).Column("\"ObjectClass_ID\"");

            HasMany<ObjectparameterPerProcess>(x => x.ProcessParameters).KeyColumn("\"Objektparameter_ID\"");

            HasMany<ObjectparameterHasProperties>(x => x.HasProperties).KeyColumn("\"Objectparameter_ID\"");
        }
    }
}