namespace DbExperiments.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PasswordAccessibilityChanged : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "EncodedPassword", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "EncodedPassword");
        }
    }
}
