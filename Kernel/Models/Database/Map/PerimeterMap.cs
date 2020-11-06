using ResTB_API.Models.Database.Domain;
using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;

namespace ResTB_API.Models.Database.Map
{


    public class PerimeterMap : ClassMap<Perimeter>
    {

        public PerimeterMap()
        {
            Schema("public");
            Table("\"Perimeter\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.Id).GeneratedBy.Identity().Column("\"fid\"");
            Map(x => x.geometry).Column("\"geometry\"").CustomType<GeometryType>();
            References<Project>(x => x.Project).Column("\"project_fk\""); 

        }
    }
}
