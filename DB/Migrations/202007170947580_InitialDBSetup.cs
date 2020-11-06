namespace ResTB.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDBSetup : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.DamageExtent",
                c => new
                    {
                        MappedObjectId = c.Int(nullable: false),
                        IntensityId = c.Int(nullable: false),
                        Area = c.Double(nullable: false),
                        Length = c.Double(nullable: false),
                        Piece = c.Double(nullable: false),
                        Clipped = c.Boolean(nullable: false),
                        PersonDamage = c.Double(nullable: false),
                        LogPersonDamage = c.String(),
                        PropertyDamage = c.Double(nullable: false),
                        LogPropertyDamage = c.String(),
                        Deaths = c.Double(nullable: false),
                        LogDeaths = c.String(),
                        DeathProbability = c.Double(nullable: false),
                        LogDeathProbability = c.String(),
                        IndirectDamage = c.Double(nullable: false),
                        LogIndirectDamage = c.String(),
                        ResilienceFactor = c.Double(nullable: false),
                        LogResilienceFactor = c.String(),
                        Log = c.String(),
                    })
                .PrimaryKey(t => new { t.MappedObjectId, t.IntensityId })
                .ForeignKey("public.Intensity", t => t.IntensityId)
                .ForeignKey("public.MappedObject", t => t.MappedObjectId)
                .Index(t => t.MappedObjectId)
                .Index(t => t.IntensityId);
            
            CreateTable(
                "public.Intensity",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        BeforeAction = c.Boolean(nullable: false),
                        IntensityDegree = c.Int(nullable: false),
                        NatHazard_ID = c.Int(),
                        Project_Id = c.Int(),
                        IKClasses_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.NatHazard", t => t.NatHazard_ID)
                .ForeignKey("public.Project", t => t.Project_Id)
                .ForeignKey("public.IKClasses", t => t.IKClasses_ID)
                .Index(t => t.NatHazard_ID)
                .Index(t => t.Project_Id)
                .Index(t => t.IKClasses_ID);
            
            CreateTable(
                "public.IKClasses",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        Value = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "public.PrA",
                c => new
                    {
                        NatHazardId = c.Int(nullable: false),
                        IKClassesId = c.Int(nullable: false),
                        ProjectId = c.Int(nullable: false),
                        Value = c.Double(nullable: false),
                    })
                .PrimaryKey(t => new { t.NatHazardId, t.IKClassesId, t.ProjectId })
                .ForeignKey("public.IKClasses", t => t.IKClassesId)
                .ForeignKey("public.Project", t => t.ProjectId, cascadeDelete: true)
                .ForeignKey("public.NatHazard", t => t.NatHazardId)
                .Index(t => t.NatHazardId)
                .Index(t => t.IKClassesId)
                .Index(t => t.ProjectId);
            
            CreateTable(
                "public.NatHazard",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "public.ObjectparameterPerProcess",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        VulnerabilityLow = c.Double(nullable: false),
                        VulnerabilityMedium = c.Double(nullable: false),
                        VulnerabilityHigh = c.Double(nullable: false),
                        MortalityLow = c.Double(nullable: false),
                        MortalityMedium = c.Double(nullable: false),
                        MortalityHigh = c.Double(nullable: false),
                        Value = c.Double(nullable: false),
                        Unit = c.String(),
                        IndirectVulnerabilityLow = c.Double(nullable: false),
                        IndirectVulnerabilityMedium = c.Double(nullable: false),
                        IndirectVulnerabilityHigh = c.Double(nullable: false),
                        DurationLow = c.Double(nullable: false),
                        DurationMedium = c.Double(nullable: false),
                        DurationHigh = c.Double(nullable: false),
                        NatHazard_ID = c.Int(),
                        Objektparameter_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.NatHazard", t => t.NatHazard_ID)
                .ForeignKey("public.Objectparameter", t => t.Objektparameter_ID)
                .Index(t => t.NatHazard_ID)
                .Index(t => t.Objektparameter_ID);
            
            CreateTable(
                "public.Objectparameter",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        FeatureType = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        Value = c.Int(nullable: false),
                        ChangeValue = c.String(),
                        Unity = c.String(),
                        Floors = c.Int(nullable: false),
                        Personcount = c.Int(nullable: false),
                        ChangePersonCount = c.String(),
                        Presence = c.Double(nullable: false),
                        NumberOfVehicles = c.Int(nullable: false),
                        Velocity = c.Int(nullable: false),
                        Staff = c.Int(nullable: false),
                        IsStandard = c.Boolean(nullable: false),
                        MotherOtbjectparameter_ID = c.Int(),
                        ObjectClass_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.Objectparameter", t => t.MotherOtbjectparameter_ID)
                .ForeignKey("public.ObjectClass", t => t.ObjectClass_ID)
                .Index(t => t.MotherOtbjectparameter_ID)
                .Index(t => t.ObjectClass_ID);
            
            CreateTable(
                "public.ObjectparameterHasProperties",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Property = c.String(),
                        isOptional = c.Boolean(nullable: false),
                        Objectparameter_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.Objectparameter", t => t.Objectparameter_ID)
                .Index(t => t.Objectparameter_ID);
            
            CreateTable(
                "public.ObjectClass",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "public.ResilienceFactor",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        Preparedness = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "public.ResilienceWeight",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Weight = c.Double(nullable: false),
                        BeforeAction = c.Boolean(),
                        NatHazard_ID = c.Int(),
                        ResilienceFactor_ID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.NatHazard", t => t.NatHazard_ID)
                .ForeignKey("public.ResilienceFactor", t => t.ResilienceFactor_ID)
                .Index(t => t.NatHazard_ID)
                .Index(t => t.ResilienceFactor_ID);
            
            CreateTable(
                "public.ResilienceValues",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        OverwrittenWeight = c.Double(nullable: false),
                        Value = c.Double(nullable: false),
                        MappedObject_ID = c.Int(),
                        ResilienceWeight_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.MappedObject", t => t.MappedObject_ID)
                .ForeignKey("public.ResilienceWeight", t => t.ResilienceWeight_ID)
                .Index(t => t.MappedObject_ID)
                .Index(t => t.ResilienceWeight_ID);
            
            CreateTable(
                "public.MappedObject",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        FreeFillParameter_ID = c.Int(),
                        Objectparameter_ID = c.Int(),
                        Project_Id = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.Objectparameter", t => t.FreeFillParameter_ID)
                .ForeignKey("public.Objectparameter", t => t.Objectparameter_ID)
                .ForeignKey("public.Project", t => t.Project_Id)
                .Index(t => t.FreeFillParameter_ID)
                .Index(t => t.Objectparameter_ID)
                .Index(t => t.Project_Id);
            
            CreateTable(
                "public.Project",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Number = c.String(nullable: false),
                        Description = c.String(),
                        CoordinateSystem = c.Int(nullable: false),
                        ProjectState_ID = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.ProjectState", t => t.ProjectState_ID)
                .Index(t => t.ProjectState_ID);
            
            CreateTable(
                "public.ProjectState",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "public.ProtectionMeasure",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Costs = c.Int(nullable: false),
                        LifeSpan = c.Int(nullable: false),
                        OperatingCosts = c.Int(nullable: false),
                        MaintenanceCosts = c.Int(nullable: false),
                        RateOfReturn = c.Double(nullable: false),
                        Description = c.String(),
                        ValueAddedTax = c.Double(nullable: false),
                        ProjectID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.Project", t => t.ProjectID)
                .Index(t => t.ProjectID);
            
            CreateTable(
                "public.PostGISHatObjektparameter",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PostGISID = c.Int(nullable: false),
                        Objektparameter_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.Objectparameter", t => t.Objektparameter_ID)
                .Index(t => t.Objektparameter_ID);
            
            CreateTable(
                "public.Standard_PrA",
                c => new
                    {
                        NatHazardId = c.Int(nullable: false),
                        IKClassesId = c.Int(nullable: false),
                        Value = c.Double(nullable: false),
                    })
                .PrimaryKey(t => new { t.NatHazardId, t.IKClassesId })
                .ForeignKey("public.IKClasses", t => t.IKClassesId, cascadeDelete: true)
                .ForeignKey("public.NatHazard", t => t.NatHazardId, cascadeDelete: true)
                .Index(t => t.NatHazardId)
                .Index(t => t.IKClassesId);
            
            CreateTable(
                "public.WillingnessToPay",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Value = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "public.ObjectparameterHasResilienceFactors",
                c => new
                    {
                        ResilienceFactor_ID = c.String(nullable: false, maxLength: 128),
                        Objectparameter_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ResilienceFactor_ID, t.Objectparameter_ID })
                .ForeignKey("public.ResilienceFactor", t => t.ResilienceFactor_ID, cascadeDelete: true)
                .ForeignKey("public.Objectparameter", t => t.Objectparameter_ID, cascadeDelete: true)
                .Index(t => t.ResilienceFactor_ID)
                .Index(t => t.Objectparameter_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.Standard_PrA", "NatHazardId", "public.NatHazard");
            DropForeignKey("public.Standard_PrA", "IKClassesId", "public.IKClasses");
            DropForeignKey("public.PostGISHatObjektparameter", "Objektparameter_ID", "public.Objectparameter");
            DropForeignKey("public.DamageExtent", "MappedObjectId", "public.MappedObject");
            DropForeignKey("public.DamageExtent", "IntensityId", "public.Intensity");
            DropForeignKey("public.Intensity", "IKClasses_ID", "public.IKClasses");
            DropForeignKey("public.PrA", "NatHazardId", "public.NatHazard");
            DropForeignKey("public.ResilienceValues", "ResilienceWeight_ID", "public.ResilienceWeight");
            DropForeignKey("public.ResilienceValues", "MappedObject_ID", "public.MappedObject");
            DropForeignKey("public.ProtectionMeasure", "ProjectID", "public.Project");
            DropForeignKey("public.Project", "ProjectState_ID", "public.ProjectState");
            DropForeignKey("public.PrA", "ProjectId", "public.Project");
            DropForeignKey("public.MappedObject", "Project_Id", "public.Project");
            DropForeignKey("public.Intensity", "Project_Id", "public.Project");
            DropForeignKey("public.MappedObject", "Objectparameter_ID", "public.Objectparameter");
            DropForeignKey("public.MappedObject", "FreeFillParameter_ID", "public.Objectparameter");
            DropForeignKey("public.ResilienceWeight", "ResilienceFactor_ID", "public.ResilienceFactor");
            DropForeignKey("public.ResilienceWeight", "NatHazard_ID", "public.NatHazard");
            DropForeignKey("public.ObjectparameterHasResilienceFactors", "Objectparameter_ID", "public.Objectparameter");
            DropForeignKey("public.ObjectparameterHasResilienceFactors", "ResilienceFactor_ID", "public.ResilienceFactor");
            DropForeignKey("public.ObjectparameterPerProcess", "Objektparameter_ID", "public.Objectparameter");
            DropForeignKey("public.Objectparameter", "ObjectClass_ID", "public.ObjectClass");
            DropForeignKey("public.Objectparameter", "MotherOtbjectparameter_ID", "public.Objectparameter");
            DropForeignKey("public.ObjectparameterHasProperties", "Objectparameter_ID", "public.Objectparameter");
            DropForeignKey("public.ObjectparameterPerProcess", "NatHazard_ID", "public.NatHazard");
            DropForeignKey("public.Intensity", "NatHazard_ID", "public.NatHazard");
            DropForeignKey("public.PrA", "IKClassesId", "public.IKClasses");
            DropIndex("public.ObjectparameterHasResilienceFactors", new[] { "Objectparameter_ID" });
            DropIndex("public.ObjectparameterHasResilienceFactors", new[] { "ResilienceFactor_ID" });
            DropIndex("public.Standard_PrA", new[] { "IKClassesId" });
            DropIndex("public.Standard_PrA", new[] { "NatHazardId" });
            DropIndex("public.PostGISHatObjektparameter", new[] { "Objektparameter_ID" });
            DropIndex("public.ProtectionMeasure", new[] { "ProjectID" });
            DropIndex("public.Project", new[] { "ProjectState_ID" });
            DropIndex("public.MappedObject", new[] { "Project_Id" });
            DropIndex("public.MappedObject", new[] { "Objectparameter_ID" });
            DropIndex("public.MappedObject", new[] { "FreeFillParameter_ID" });
            DropIndex("public.ResilienceValues", new[] { "ResilienceWeight_ID" });
            DropIndex("public.ResilienceValues", new[] { "MappedObject_ID" });
            DropIndex("public.ResilienceWeight", new[] { "ResilienceFactor_ID" });
            DropIndex("public.ResilienceWeight", new[] { "NatHazard_ID" });
            DropIndex("public.ObjectparameterHasProperties", new[] { "Objectparameter_ID" });
            DropIndex("public.Objectparameter", new[] { "ObjectClass_ID" });
            DropIndex("public.Objectparameter", new[] { "MotherOtbjectparameter_ID" });
            DropIndex("public.ObjectparameterPerProcess", new[] { "Objektparameter_ID" });
            DropIndex("public.ObjectparameterPerProcess", new[] { "NatHazard_ID" });
            DropIndex("public.PrA", new[] { "ProjectId" });
            DropIndex("public.PrA", new[] { "IKClassesId" });
            DropIndex("public.PrA", new[] { "NatHazardId" });
            DropIndex("public.Intensity", new[] { "IKClasses_ID" });
            DropIndex("public.Intensity", new[] { "Project_Id" });
            DropIndex("public.Intensity", new[] { "NatHazard_ID" });
            DropIndex("public.DamageExtent", new[] { "IntensityId" });
            DropIndex("public.DamageExtent", new[] { "MappedObjectId" });
            DropTable("public.ObjectparameterHasResilienceFactors");
            DropTable("public.WillingnessToPay");
            DropTable("public.Standard_PrA");
            DropTable("public.PostGISHatObjektparameter");
            DropTable("public.ProtectionMeasure");
            DropTable("public.ProjectState");
            DropTable("public.Project");
            DropTable("public.MappedObject");
            DropTable("public.ResilienceValues");
            DropTable("public.ResilienceWeight");
            DropTable("public.ResilienceFactor");
            DropTable("public.ObjectClass");
            DropTable("public.ObjectparameterHasProperties");
            DropTable("public.Objectparameter");
            DropTable("public.ObjectparameterPerProcess");
            DropTable("public.NatHazard");
            DropTable("public.PrA");
            DropTable("public.IKClasses");
            DropTable("public.Intensity");
            DropTable("public.DamageExtent");
        }
    }
}
