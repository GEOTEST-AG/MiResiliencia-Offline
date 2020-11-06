using ResTB_API.Models.Database.Domain;
using FluentNHibernate.Mapping;

namespace ResTB_API.Models.Database.Map
{
    public class ProjectStateMap : ClassMap<ProjectState>
    {
        public ProjectStateMap()
        {
            Schema("public");
            Table("\"ProjectState\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).Column("\"ID\"").GeneratedBy.Assigned();
            Map(x => x.Description).Column("\"Description\"");
        }
    }
}
