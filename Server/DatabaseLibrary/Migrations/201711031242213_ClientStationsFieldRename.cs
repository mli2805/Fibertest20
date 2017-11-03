namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ClientStationsFieldRename : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClientStations", "StationIp", c => c.String(unicode: false));
            DropColumn("dbo.ClientStations", "StationIpAddress");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ClientStations", "StationIpAddress", c => c.String(unicode: false));
            DropColumn("dbo.ClientStations", "StationIp");
        }
    }
}
