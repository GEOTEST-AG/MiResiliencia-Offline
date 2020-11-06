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


    public class PrAMap : ClassMap<PrA>
    {

        public PrAMap()
        {
            Schema("public");
            Table("\"PrA\"");
            LazyLoad();
            ReadOnly();

            CompositeId()
                .KeyReference(x => x.NatHazard, "\"NatHazardId\"")
                .KeyReference(x => x.IKClass, "\"IKClassesId\"")
                .KeyReference(x => x.Project, "\"ProjectId\"");

            Map(x => x.Value).Column("\"Value\"");

        }
    }
}
