namespace ResTB.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Translation_Name : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.Objectparameter", "Name_EN", c => c.String());
            AddColumn("public.Objectparameter", "Name_ES", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("public.Objectparameter", "Name_ES");
            DropColumn("public.Objectparameter", "Name_EN");
        }
    }
}
