namespace DbExperiments.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PasswordNameChanged : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Users", "PasswordInDb");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "PasswordInDb", c => c.String(unicode: false));
        }
    }
}
