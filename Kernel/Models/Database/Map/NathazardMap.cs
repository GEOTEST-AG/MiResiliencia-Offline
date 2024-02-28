using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using ResTB_API.Models.Database.Domain;
using FluentNHibernate.Mapping;

namespace ResTB_API.Models.Database.Map
{
    public class NathazardMap : ClassMap<NatHazard>
    {
        public NathazardMap()
        {
            Schema("public");
            Table("\"NatHazard\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).GeneratedBy.Assigned().Column("\"ID\"");
            Map(x => x.Name).Column("\"Name\"");
            Map(x => x.Name_EN).Column("\"Name_EN\"");
            Map(x => x.Name_ES).Column("\"Name_ES\"");

        }
    }
}
