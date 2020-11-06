using FluentNHibernate.Mapping;
using ResTB_API.Models.Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models.Database.Map
{
    public class ObjectparameterperprocessMap : ClassMap<ObjectparameterPerProcess>
    {

        public ObjectparameterperprocessMap()
        {
            Schema("public");
            Table("\"ObjectparameterPerProcess\"");
            LazyLoad();
            ReadOnly();
            Id(x => x.ID).GeneratedBy.Identity().Column("\"ID\"");

            Map(x => x.MortalityLow).Column($"\"{nameof(ObjectparameterPerProcess.MortalityLow)}\""); ;
            Map(x => x.MortalityMedium).Column($"\"{nameof(ObjectparameterPerProcess.MortalityMedium)}\"");
            Map(x => x.MortalityHigh).Column($"\"{nameof(ObjectparameterPerProcess.MortalityHigh)}\"");

            Map(x => x.VulnerabilityLow).Column($"\"{nameof(ObjectparameterPerProcess.VulnerabilityLow)}\"");
            Map(x => x.VulnerabilityMedium).Column($"\"{nameof(ObjectparameterPerProcess.VulnerabilityMedium)}\""); ;
            Map(x => x.VulnerabilityHigh).Column($"\"{nameof(ObjectparameterPerProcess.VulnerabilityHigh)}\""); 

            Map(x=>x.Value).Column($"\"{nameof(ObjectparameterPerProcess.Value)}\"");
            Map(x=>x.Unit).Column($"\"{nameof(ObjectparameterPerProcess.Unit)}\"");

            Map(x => x.DurationLow).Column($"\"{nameof(ObjectparameterPerProcess.DurationLow)}\"");
            Map(x => x.DurationMedium).Column($"\"{nameof(ObjectparameterPerProcess.DurationMedium)}\"");
            Map(x => x.DurationHigh).Column($"\"{nameof(ObjectparameterPerProcess.DurationHigh)}\"");

            //Map(x => x.IndirectVulnerabilityLow).Column($"\"{nameof(ObjectparameterPerProcess.IndirectVulnerabilityLow)}\"");
            //Map(x => x.IndirectVulnerabilityMedium).Column($"\"{nameof(ObjectparameterPerProcess.IndirectVulnerabilityMedium)}\"");
            //Map(x => x.IndirectVulnerabilityHigh).Column($"\"{nameof(ObjectparameterPerProcess.IndirectVulnerabilityHigh)}\"");

            References<Objectparameter>(x => x.Objectparameter).Column("\"Objektparameter_ID\"");
            References<NatHazard>(x => x.NatHazard).Column("\"NatHazard_ID\"");
        }
    }
}