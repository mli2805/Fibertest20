namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsReserveAddressSetAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RtuStations", "IsReserveAddressSet", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RtuStations", "IsReserveAddressSet");
        }
    }
}
