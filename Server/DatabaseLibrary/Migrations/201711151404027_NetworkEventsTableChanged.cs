namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class NetworkEventsTableChanged : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NetworkEvents", "MainChannelState", c => c.Int(nullable: false));
            AddColumn("dbo.NetworkEvents", "ReserveChannelState", c => c.Int(nullable: false));
            AddColumn("dbo.NetworkEvents", "BopString", c => c.String(unicode: false));
            DropColumn("dbo.NetworkEvents", "Part");
            DropColumn("dbo.NetworkEvents", "PartState");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NetworkEvents", "PartState", c => c.Int(nullable: false));
            AddColumn("dbo.NetworkEvents", "Part", c => c.Int(nullable: false));
            DropColumn("dbo.NetworkEvents", "BopString");
            DropColumn("dbo.NetworkEvents", "ReserveChannelState");
            DropColumn("dbo.NetworkEvents", "MainChannelState");
        }
    }
}
