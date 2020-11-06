using FluentNHibernate.Mapping;
using ResTB_API.Models.Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models.Database.Map
{
    public class ObjectparameterHasPropertiesMap : ClassMap<ObjectparameterHasProperties>
    {
        public ObjectparameterHasPropertiesMap()
        {
            Schema("public");
            Table("\"ObjectparameterHasProperties\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).GeneratedBy.Assigned().Column("\"ID\"");
            Map(x => x.Property).Column("\"Property\"");
            Map(x => x.isOptional).Column("\"isOptional\"");
            References<Objectparameter>(x => x.Objectparameter).Column("\"Objectparameter_ID\"");
        }
    }
   
}