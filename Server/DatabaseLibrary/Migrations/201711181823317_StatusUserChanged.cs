namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class StatusUserChanged : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OpticalEvents", "StatusUser", c => c.String(unicode: false));
            DropColumn("dbo.OpticalEvents", "StatusUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OpticalEvents", "StatusUserId", c => c.Int(nullable: false));
            DropColumn("dbo.OpticalEvents", "StatusUser");
        }
    }
}
