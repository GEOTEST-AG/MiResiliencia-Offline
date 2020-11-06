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


    public class ResilienceValuesMap : ClassMap<ResilienceValues>
    {
        public ResilienceValuesMap()
        {
            Schema("public");
            Table("\"ResilienceValues\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).GeneratedBy.Identity().Column("\"ID\"");
            Map(x => x.Value).Column("\"Value\""); ;
            References<MappedObject>(x => x.MappedObject).Column("\"MappedObject_ID\"");
            References<ResilienceWeight>(x => x.ResilienceWeight).Column("\"ResilienceWeight_ID\""); ;
            Map(x => x.OverwrittenWeight).Column("\"OverwrittenWeight\""); ;
        }
    }
}
