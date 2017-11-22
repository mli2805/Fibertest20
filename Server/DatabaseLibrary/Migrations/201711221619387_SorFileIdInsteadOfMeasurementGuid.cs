namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class SorFileIdInsteadOfMeasurementGuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Measurements", "SorFileId", c => c.Int(nullable: false));
            AddColumn("dbo.OpticalEvents", "SorFileId", c => c.Int(nullable: false));
            DropColumn("dbo.Measurements", "MeasurementId");
            DropColumn("dbo.OpticalEvents", "MeasurementId");
            DropColumn("dbo.SorFiles", "MeasurementId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SorFiles", "MeasurementId", c => c.Guid(nullable: false));
            AddColumn("dbo.OpticalEvents", "MeasurementId", c => c.Guid(nullable: false));
            AddColumn("dbo.Measurements", "MeasurementId", c => c.Guid(nullable: false));
            DropColumn("dbo.OpticalEvents", "SorFileId");
            DropColumn("dbo.Measurements", "SorFileId");
        }
    }
}
