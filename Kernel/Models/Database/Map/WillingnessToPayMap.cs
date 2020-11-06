using ResTB_API.Models.Database.Domain;
using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;

namespace ResTB_API.Models.Database.Map
{


    public class WillingnessToPayMap : ClassMap<WillingnessToPay>
    {

        public WillingnessToPayMap()
        {
            Schema("public");
            Table("\"WillingnessToPay\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).GeneratedBy.Identity().Column("\"ID\"");
            Map(x => x.Value).Column("\"Value\"");
        }
    }
}
