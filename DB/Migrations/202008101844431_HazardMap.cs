namespace ResTB.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HazardMap : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.HazardMap",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Index = c.Int(nullable: false),
                        BeforeAction = c.Boolean(nullable: false),
                        NatHazard_ID = c.Int(),
                        Project_Id = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("public.NatHazard", t => t.NatHazard_ID)
                .ForeignKey("public.Project", t => t.Project_Id)
                .Index(t => t.NatHazard_ID)
                .Index(t => t.Project_Id);
            
            AddColumn("public.DamageExtent", "HazardMap_ID", c => c.Int());
            CreateIndex("public.DamageExtent", "HazardMap_ID");
            AddForeignKey("public.DamageExtent", "HazardMap_ID", "public.HazardMap", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("public.HazardMap", "Project_Id", "public.Project");
            DropForeignKey("public.HazardMap", "NatHazard_ID", "public.NatHazard");
            DropForeignKey("public.DamageExtent", "HazardMap_ID", "public.HazardMap");
            DropIndex("public.HazardMap", new[] { "Project_Id" });
            DropIndex("public.HazardMap", new[] { "NatHazard_ID" });
            DropIndex("public.DamageExtent", new[] { "HazardMap_ID" });
            DropColumn("public.DamageExtent", "HazardMap_ID");
            DropTable("public.HazardMap");
        }
    }
}
