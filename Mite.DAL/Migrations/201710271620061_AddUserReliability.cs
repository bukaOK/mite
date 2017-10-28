namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserReliability : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Reliability", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Reliability");
        }
    }
}
