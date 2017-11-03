namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientStations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StationId = c.Guid(nullable: false),
                        Username = c.String(unicode: false),
                        StationIpAddress = c.String(unicode: false),
                        LastConnectionTimestamp = c.DateTime(nullable: false, precision: 0),
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
            DropTable("dbo.ClientStations");
        }
    }
}
