using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using ResTB_API.Models.Database.Domain ;
using FluentNHibernate.Mapping;

namespace ResTB_API.Models.Database.Map {
    
    
    public class IKClassesMap : ClassMap<IKClasses> {
        
        public IKClassesMap() {
			Schema("public");
            Table("\"IKClasses\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).Column("\"ID\"").GeneratedBy.Assigned();
			Map(x => x.Description).Column("\"Description\"");
			Map(x => x.Value).Column("\"Value\"");
        }
    }
}
