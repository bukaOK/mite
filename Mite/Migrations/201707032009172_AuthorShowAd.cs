namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuthorShowAd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ShowAd", c => c.Boolean(nullable: false));
            DropColumn("dbo.AspNetUsers", "FollowIds");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "FollowIds", c => c.String());
            DropColumn("dbo.AspNetUsers", "ShowAd");
        }
    }
}
