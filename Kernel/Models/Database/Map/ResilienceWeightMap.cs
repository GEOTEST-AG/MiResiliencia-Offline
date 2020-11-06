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


    public class ResilienceWeightMap : ClassMap<ResilienceWeight>
    {
        public ResilienceWeightMap()
        {
            Schema("public");
            Table("\"ResilienceWeight\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).GeneratedBy.Identity().Column("\"ID\"");
            Map(x => x.Weight).Column("\"Weight\""); ;
            References<NatHazard>(x => x.NatHazard).Column("\"NatHazard_ID\"");
            Map(x => x.ResilienceFactor_ID).Column("\"ResilienceFactor_ID\"");
            Map(x=>x.BeforeAction).Column("\"BeforeAction\"");
        }
    }
}
