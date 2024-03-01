namespace ResTB.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Translations : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.IKClasses", "Description_EN", c => c.String());
            AddColumn("public.IKClasses", "Description_ES", c => c.String());
            AddColumn("public.NatHazard", "Name_EN", c => c.String());
            AddColumn("public.NatHazard", "Name_ES", c => c.String());
            AddColumn("public.Objectparameter", "Description_EN", c => c.String());
            AddColumn("public.Objectparameter", "Description_ES", c => c.String());
            AddColumn("public.ObjectClass", "Name_EN", c => c.String());
            AddColumn("public.ObjectClass", "Name_ES", c => c.String());
            AddColumn("public.ResilienceFactor", "Preparedness_EN", c => c.String());
            AddColumn("public.ResilienceFactor", "Preparedness_ES", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("public.ResilienceFactor", "Preparedness_ES");
            DropColumn("public.ResilienceFactor", "Preparedness_EN");
            DropColumn("public.ObjectClass", "Name_ES");
            DropColumn("public.ObjectClass", "Name_EN");
            DropColumn("public.Objectparameter", "Description_ES");
            DropColumn("public.Objectparameter", "Description_EN");
            DropColumn("public.NatHazard", "Name_ES");
            DropColumn("public.NatHazard", "Name_EN");
            DropColumn("public.IKClasses", "Description_ES");
            DropColumn("public.IKClasses", "Description_EN");
        }
    }
}
