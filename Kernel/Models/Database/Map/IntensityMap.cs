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
    public class IntensityMap : ClassMap<Intensity>
    {
        public IntensityMap()
        {
            Schema("public");
            Table("\"Intensity\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).GeneratedBy.Identity().Column("\"ID\"");
            Map(x => x.BeforeAction).Column("\"BeforeAction\""); ;
            Map(x => x.geometry).Column("\"geometry\"").CustomType<MyPostGisGeometryType>();
            Map(x => x.IntensityDegree).Column("\"IntensityDegree\""); ;
            References<NatHazard>(x => x.NatHazard).Column("\"NatHazard_ID\"");
            References<Project>(x => x.Project).Column("\"Project_Id\""); ;
            References<IKClasses>(x => x.IKClasses).Column("\"IKClasses_ID\""); ;

        }
    }
}
