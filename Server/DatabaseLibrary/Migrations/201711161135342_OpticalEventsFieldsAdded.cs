namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OpticalEventsFieldsAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OpticalEvents", "BaseRefType", c => c.Int(nullable: false));
            AddColumn("dbo.OpticalEvents", "Comment", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OpticalEvents", "Comment");
            DropColumn("dbo.OpticalEvents", "BaseRefType");
        }
    }
}
