using ResTB_API.Models.Database.Domain;
using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;

namespace ResTB_API.Models.Database.Map
{


    public class ProjectMap : ClassMap<Project>
    {

        public ProjectMap()
        {
            Schema("public");
            Table("\"Project\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.Id).GeneratedBy.Identity().Column("\"Id\"");
            Map(x => x.geometry).Column("\"geometry\"").CustomType<GeometryType>();
            Map(x => x.Name).Column("\"Name\"");
            Map(x => x.Number).Column("\"Number\"");
            Map(x => x.CoordinateSystem).Column("\"CoordinateSystem\"");
            Map(x => x.Description).Column("\"Description\"");

            HasMany<PrA>(x => x.PrAs).KeyColumn("\"ProjectId\"");

            HasMany<ProtectionMeasure>(x => x.ProtectionMeasures).KeyColumn("\"ProjectID\"");

            //References<Company>(x => x.Company).Column("\"Company_ID\"");

            References<ProjectState>(x => x.ProjectState).Column("\"ProjectState_ID\"");

        }
    }
}
