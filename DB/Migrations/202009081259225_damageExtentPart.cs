namespace ResTB.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class damageExtentPart : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.DamageExtent", "Part", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.DamageExtent", "Part");
        }
    }
}
