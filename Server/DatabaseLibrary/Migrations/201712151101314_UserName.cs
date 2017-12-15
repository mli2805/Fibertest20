namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class UserName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BaseRefs", "UserName", c => c.String(unicode: false));
            AddColumn("dbo.ClientStations", "UserName", c => c.String(unicode: false));
            DropColumn("dbo.BaseRefs", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BaseRefs", "UserId", c => c.Int(nullable: false));
            DropColumn("dbo.ClientStations", "UserName");
            DropColumn("dbo.BaseRefs", "UserName");
        }
    }
}
