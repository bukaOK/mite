namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPublishDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "PublishDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "PublishDate");
        }
    }
}
