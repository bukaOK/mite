namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPostCover : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "Cover", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "Cover");
        }
    }
}
