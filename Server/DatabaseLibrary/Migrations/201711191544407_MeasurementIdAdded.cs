namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class MeasurementIdAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OpticalEvents", "MeasurementId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OpticalEvents", "MeasurementId");
        }
    }
}
