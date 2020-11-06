using FluentNHibernate.Mapping;
using ResTB_API.Models.Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Spatial.Type;

namespace ResTB_API.Models.Database.Map
{
    public class ProtectionMeasureMap : ClassMap<ProtectionMeasure>
    {

        public ProtectionMeasureMap()
        {
            Schema("public");
            Table("\"ProtectionMeasure\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).GeneratedBy.Identity().Column("\"ID\"");

            Map(x => x.Costs).Column("\"Costs\"");
            Map(x => x.LifeSpan).Column("\"LifeSpan\"");
            Map(x => x.OperatingCosts).Column("\"OperatingCosts\"");
            Map(x => x.MaintenanceCosts).Column("\"MaintenanceCosts\"");
            Map(x => x.RateOfReturn).Column("\"RateOfReturn\"");
            Map(x => x.ValueAddedTax).Column("\"ValueAddedTax\"");
            Map(x => x.Description).Column("\"Description\"");
            //Map(x => x.geometry).Column("\"geometry\"").CustomType<GeometryType>();

            References<Project>(x => x.Project).Column("\"ProjectID\"");
        }
    }
}