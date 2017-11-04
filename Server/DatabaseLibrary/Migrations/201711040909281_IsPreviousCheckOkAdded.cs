namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsPreviousCheckOkAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RtuStations", "IsMainAddressOkDuePreviousCheck", c => c.Boolean(nullable: false));
            AddColumn("dbo.RtuStations", "IsReserveAddressOkDuePreviousCheck", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RtuStations", "IsReserveAddressOkDuePreviousCheck");
            DropColumn("dbo.RtuStations", "IsMainAddressOkDuePreviousCheck");
        }
    }
}
