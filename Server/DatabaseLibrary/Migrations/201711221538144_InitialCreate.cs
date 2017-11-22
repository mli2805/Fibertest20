namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BaseRefs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BaseRefId = c.Guid(nullable: false),
                        TraceId = c.Guid(nullable: false),
                        BaseRefType = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        SaveTimestamp = c.DateTime(nullable: false, precision: 0),
                        SorBytes = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ClientStations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientGuid = c.Guid(nullable: false),
                        UserId = c.Int(nullable: false),
                        ClientAddress = c.String(unicode: false),
                        ClientAddressPort = c.Int(nullable: false),
                        LastConnectionTimestamp = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.Id);
            
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
                        MainChannelState = c.Int(nullable: false),
                        ReserveChannelState = c.Int(nullable: false),
                        BopString = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OpticalEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventRegistrationTimestamp = c.DateTime(nullable: false, precision: 0),
                        RtuId = c.Guid(nullable: false),
                        TraceId = c.Guid(nullable: false),
                        BaseRefType = c.Int(nullable: false),
                        TraceState = c.Int(nullable: false),
                        EventStatus = c.Int(nullable: false),
                        StatusChangedTimestamp = c.DateTime(nullable: false, precision: 0),
                        StatusChangedByUser = c.String(unicode: false),
                        Comment = c.String(unicode: false),
                        MeasurementId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RtuStations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RtuGuid = c.Guid(nullable: false),
                        Version = c.String(unicode: false),
                        MainAddress = c.String(unicode: false),
                        MainAddressPort = c.Int(nullable: false),
                        LastConnectionByMainAddressTimestamp = c.DateTime(nullable: false, precision: 0),
                        IsMainAddressOkDuePreviousCheck = c.Boolean(nullable: false),
                        IsReserveAddressSet = c.Boolean(nullable: false),
                        ReserveAddress = c.String(unicode: false),
                        ReserveAddressPort = c.Int(nullable: false),
                        LastConnectionByReserveAddressTimestamp = c.DateTime(nullable: false, precision: 0),
                        IsReserveAddressOkDuePreviousCheck = c.Boolean(nullable: false),
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
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        EncodedPassword = c.String(unicode: false),
                        Email = c.String(unicode: false),
                        IsEmailActivated = c.Boolean(nullable: false),
                        Role = c.Int(nullable: false),
                        ZoneId = c.Guid(nullable: false),
                        IsDefaultZoneUser = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
            DropTable("dbo.SorFiles");
            DropTable("dbo.RtuStations");
            DropTable("dbo.OpticalEvents");
            DropTable("dbo.NetworkEvents");
            DropTable("dbo.Measurements");
            DropTable("dbo.ClientStations");
            DropTable("dbo.BaseRefs");
        }
    }
}
