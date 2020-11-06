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
    public class MappedObjectMap : ClassMap<MappedObject>
    {
        public MappedObjectMap()
        {
            Schema("public");
            Table("\"MappedObject\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).Column("\"ID\"");            
            Map(x => x.point).Column("\"point\"").CustomType<MyPostGisGeometryType>();
            Map(x => x.line).Column("\"line\"").CustomType<MyPostGisGeometryType>();
            Map(x => x.polygon).Column("\"polygon\"").CustomType<MyPostGisGeometryType>();
            References<Objectparameter>(x => x.Objectparameter).Column("\"Objectparameter_ID\"");
            References<Objectparameter>(x => x.FreeFillParameter).Column("\"FreeFillParameter_ID\"").Nullable();
            References<Project>(x => x.Project).Column("\"Project_Id\"");
            HasMany<ResilienceValues>(x => x.ResilienceValues).KeyColumn("\"MappedObject_ID\"");
        }
    }
}
