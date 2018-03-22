namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ShowOnlyFollowings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "ShowOnlyFollowings", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ShowOnlyFollowings");
        }
    }
}
