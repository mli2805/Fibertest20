namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RtuStationsTableAdded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RtuStations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StationId = c.Guid(nullable: false),
                        Version = c.String(unicode: false),
                        MainAddress = c.String(unicode: false),
                        MainAddressPort = c.Int(nullable: false),
                        LastConnectionByMainAddressTimestamp = c.DateTime(nullable: false, precision: 0),
                        ReserveAddress = c.String(unicode: false),
                        ReserveAddressPort = c.Int(nullable: false),
                        LastConnectionByReserveAddressTimestamp = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RtuStations");
        }
    }
}
