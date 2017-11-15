namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MeasurementsAndEventsTablesAdded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Measurements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MeasurementId = c.Guid(nullable: false),
                        RtuId = c.Guid(nullable: false),
                        TraceId = c.Guid(nullable: false),
                        BaseRefType = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false, precision: 0),
                        TraceState = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NetworkEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventTimestamp = c.DateTime(nullable: false, precision: 0),
                        RtuId = c.Guid(nullable: false),
                        Part = c.Int(nullable: false),
                        PartState = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OpticalEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventTimestamp = c.DateTime(nullable: false, precision: 0),
                        RtuId = c.Guid(nullable: false),
                        TraceId = c.Guid(nullable: false),
                        TraceState = c.Int(nullable: false),
                        EventStatus = c.Int(nullable: false),
                        StatusTimestamp = c.DateTime(nullable: false, precision: 0),
                        StatusUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SorFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MeasurementId = c.Guid(nullable: false),
                        SorBytes = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SorFiles");
            DropTable("dbo.OpticalEvents");
            DropTable("dbo.NetworkEvents");
            DropTable("dbo.Measurements");
        }
    }
}
