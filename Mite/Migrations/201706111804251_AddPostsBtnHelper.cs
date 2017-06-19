namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPostsBtnHelper : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Helpers", "PublicPostsBtn", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Helpers", "PublicPostsBtn");
        }
    }
}
