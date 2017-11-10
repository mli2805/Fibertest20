namespace Iit.Fibertest.DatabaseLibrary.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class BaseRefsTableAdded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BaseRefs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BaseRefId = c.Guid(nullable: false),
                        TraceId = c.Guid(nullable: false),
                        BaseRefType = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        SaveTimestamp = c.DateTime(nullable: false, precision: 0),
                        SorBytes = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BaseRefs");
        }
    }
}
