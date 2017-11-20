namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class FieldsRenamed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OpticalEvents", "EventRegistrationTimestamp", c => c.DateTime(nullable: false, precision: 0));
            AddColumn("dbo.OpticalEvents", "StatusChangedTimestamp", c => c.DateTime(nullable: false, precision: 0));
            AddColumn("dbo.OpticalEvents", "StatusChangedByUser", c => c.String(unicode: false));
            DropColumn("dbo.OpticalEvents", "EventTimestamp");
            DropColumn("dbo.OpticalEvents", "StatusTimestamp");
            DropColumn("dbo.OpticalEvents", "StatusUser");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OpticalEvents", "StatusUser", c => c.String(unicode: false));
            AddColumn("dbo.OpticalEvents", "StatusTimestamp", c => c.DateTime(nullable: false, precision: 0));
            AddColumn("dbo.OpticalEvents", "EventTimestamp", c => c.DateTime(nullable: false, precision: 0));
            DropColumn("dbo.OpticalEvents", "StatusChangedByUser");
            DropColumn("dbo.OpticalEvents", "StatusChangedTimestamp");
            DropColumn("dbo.OpticalEvents", "EventRegistrationTimestamp");
        }
    }
}
