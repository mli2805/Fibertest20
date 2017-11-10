namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ClientsAddressAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClientStations", "ClientGuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ClientStations", "ClientAddress", c => c.String(unicode: false));
            AddColumn("dbo.ClientStations", "ClientAddressPort", c => c.Int(nullable: false));
            AddColumn("dbo.RtuStations", "RtuGuid", c => c.Guid(nullable: false));
            DropColumn("dbo.ClientStations", "StationId");
            DropColumn("dbo.RtuStations", "StationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RtuStations", "StationId", c => c.Guid(nullable: false));
            AddColumn("dbo.ClientStations", "StationId", c => c.Guid(nullable: false));
            DropColumn("dbo.RtuStations", "RtuGuid");
            DropColumn("dbo.ClientStations", "ClientAddressPort");
            DropColumn("dbo.ClientStations", "ClientAddress");
            DropColumn("dbo.ClientStations", "ClientGuid");
        }
    }
}
