namespace ResTB.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDBSetup1 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("public.Project", "Name", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("public.Project", new[] { "Name" });
        }
    }
}
