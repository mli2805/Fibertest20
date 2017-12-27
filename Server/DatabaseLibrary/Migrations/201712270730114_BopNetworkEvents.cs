namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class BopNetworkEvents : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BopNetworkEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventTimestamp = c.DateTime(nullable: false, precision: 0),
                        BopId = c.Guid(nullable: false),
                        RtuId = c.Guid(nullable: false),
                        State = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BopNetworkEvents");
        }
    }
}
