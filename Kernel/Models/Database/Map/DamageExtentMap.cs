using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using ResTB_API.Models.Database.Domain;
using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;

namespace ResTB_API.Models.Database.Map
{


    public class DamageExtentMap : ClassMap<DamageExtent>
    {

        public DamageExtentMap()
        {
            Schema("public");
            Table("\"DamageExtent\"");
            LazyLoad();

            CompositeId()
                .KeyReference(x => x.MappedObject, "\"MappedObjectId\"")
                .KeyReference(x => x.Intensity, "\"IntensityId\"");

            Map(x => x.Log).Column("\"Log\"");
            Map(x => x.Area).Column("\"Area\"");
            Map(x => x.Length).Column("\"Length\"");
            Map(x => x.Piece).Column("\"Piece\"");
            Map(x => x.Clipped).Column("\"Clipped\"");
            Map(x => x.Part).Column("\"Part\"");
            Map(x => x.PersonDamage).Column("\"PersonDamage\"");
            Map(x => x.PropertyDamage).Column("\"PropertyDamage\"");
            Map(x => x.Deaths).Column("\"Deaths\"");
            Map(x => x.DeathProbability).Column("\"DeathProbability\"");
            Map(x => x.IndirectDamage).Column("\"IndirectDamage\"");
            Map(x => x.ResilienceFactor).Column("\"ResilienceFactor\"");
            Map(x => x.LogPersonDamage).Column("\"LogPersonDamage\"");
            Map(x => x.LogPropertyDamage).Column("\"LogPropertyDamage\"");
            Map(x => x.LogDeaths).Column("\"LogDeaths\"");
            Map(x => x.LogDeathProbability).Column("\"LogDeathProbability\"");
            Map(x => x.LogIndirectDamage).Column("\"LogIndirectDamage\"");
            Map(x => x.LogResilienceFactor).Column("\"LogResilienceFactor\"");
            Map(x => x.geometry).Column("\"geometry\"").CustomType<MyPostGisGeometryType>();
            //Map(x => x.geometry).Column("\"point\"").CustomType<MyPostGisGeometryType>();
            //Map(x => x.geometry).Column("\"line\"").CustomType<MyPostGisGeometryType>();
            //Map(x => x.geometry).Column("\"polygon\"").CustomType<MyPostGisGeometryType>();

        }
    }
}
