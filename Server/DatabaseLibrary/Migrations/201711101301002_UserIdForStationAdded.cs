namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class UserIdForStationAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClientStations", "UserId", c => c.Int(nullable: false));
            DropColumn("dbo.ClientStations", "Username");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ClientStations", "Username", c => c.String(unicode: false));
            DropColumn("dbo.ClientStations", "UserId");
        }
    }
}
