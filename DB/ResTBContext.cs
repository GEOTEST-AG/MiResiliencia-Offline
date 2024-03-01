using ResTB.DB.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB
{
    public class ResTBContext : DbContext
    {
        public ResTBContext() : base(ConnectionString())
        {
        }

        private static string ConnectionString()
        {
            string conn = "";
            if (ConfigurationManager.AppSettings["UseOfflineDB"] == "true")
                conn = ConfigurationManager.ConnectionStrings["ResTBLocalDB"].ConnectionString;
            else
                conn = ConfigurationManager.ConnectionStrings["ResTBOnlineDB"].ConnectionString;
            return conn;
        }

        public DbSet<ResTB.DB.Models.Project> Projects { get; set; }
        public DbSet<ResTB.DB.Models.Objectparameter> Objektparameter { get; set; }
        /// <summary>
        /// not in use
        /// </summary>
        public DbSet<ResTB.DB.Models.PostGISHatObjektparameter> PostGISHatObjektparameter { get; set; }
        public DbSet<ResTB.DB.Models.NatHazard> NatHazards { get; set; }
        public DbSet<ResTB.DB.Models.ObjectparameterPerProcess> ObjektparameterProProzess { get; set; }
        public DbSet<ResTB.DB.Models.ObjectClass> ObjektKlassen { get; set; }
        public DbSet<ResTB.DB.Models.MappedObject> MappedObjects { get; set; }
        public DbSet<ResTB.DB.Models.IKClasses> IntensitaetsKlassen { get; set; }

        public DbSet<ResTB.DB.Models.Intensity> Intensities { get; set; }
        public DbSet<ResTB.DB.Models.HazardMap> HazardMaps { get; set; }
        public DbSet<ResTB.DB.Models.ProjectState> ProjectStates { get; set; }
        public DbSet<ResTB.DB.Models.ProtectionMeasure> ProtectionMeasurements { get; set; }
        public DbSet<ResTB.DB.Models.ResilienceFactor> ResilienceFactors { get; set; }
        public DbSet<ResTB.DB.Models.ResilienceValues> ResilienceValues { get; set; }
        public DbSet<ResTB.DB.Models.ResilienceWeight> ResilienceWeights { get; set; }

        public DbSet<ResTB.DB.Models.PrA> PrAs { get; set; }
        public DbSet<ResTB.DB.Models.WillingnessToPay> WillingnessToPays { get; set; }
        public DbSet<ResTB.DB.Models.DamageExtent> DamageExtents { get; set; }
        public DbSet<ResTB.DB.Models.Standard_PrA> StandardPrAs { get; set; }
        public DbSet<ResTB.DB.Models.Settings> Settings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();

            modelBuilder.Entity<ResilienceFactor>()
                .HasMany<Objectparameter>(s => s.Objectparameters)
                .WithMany(c => c.ResilienceFactors)
                .Map(cs =>
                {
                    cs.ToTable("ObjectparameterHasResilienceFactors");
                });

            modelBuilder.Entity<DamageExtent>()
                .HasRequired(mo => mo.Intensity)
                .WithMany(de => de.DamageExtents).WillCascadeOnDelete(false);
            modelBuilder.Entity<DamageExtent>()
                .HasRequired(mo => mo.MappedObject)
                .WithMany(de => de.DamageExtents).WillCascadeOnDelete(false);

            modelBuilder.Entity<PrA>()
                .HasRequired(mo => mo.IKClasses)
                .WithMany(de => de.PrAs).WillCascadeOnDelete(false);

            modelBuilder.Entity<PrA>()
                .HasRequired(mo => mo.NatHazard)
                .WithMany(de => de.PrAs).WillCascadeOnDelete(false);

            modelBuilder.Entity<Project>()
                .HasOptional<ProtectionMeasure>(s => s.ProtectionMeasure)
                .WithRequired(c => c.Project)
                .Map(cs => { cs.MapKey("ProjectID"); cs.ToTable("ProtectionMeasure"); });

            base.OnModelCreating(modelBuilder);
        }
    }
}
