namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StationIpRemoved : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ClientStations", "StationIp");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ClientStations", "StationIp", c => c.String(unicode: false));
        }
    }
}
