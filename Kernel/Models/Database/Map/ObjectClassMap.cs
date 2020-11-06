using FluentNHibernate.Mapping;
using ResTB_API.Models.Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models.Database.Map
{
    public class ObjectClassMap : ClassMap<ObjectClass>
    {
        public ObjectClassMap()
        {
            Schema("public");
            Table("\"ObjectClass\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).GeneratedBy.Assigned().Column("\"ID\"");
            Map(x => x.Name).Column("\"Name\"");
        }
    }
    //public class ObjectClassLowerMap : ClassMap<ObjectClassLower>
    //{
    //    public ObjectClassLowerMap()
    //    {
    //        Schema("public");
    //        Table("\"ObjectClass\"");
    //        LazyLoad();
    //        Id(x => x.id).GeneratedBy.Assigned().Column("id");
    //        Map(x => x.name).Column("name");
    //        Map(x => x.featuretype).Column("featuretype");
    //    }
    //}
}