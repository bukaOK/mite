namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatePostTypes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "Type", c => c.Short(nullable: false));
            AddColumn("dbo.Posts", "ContentType", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "ContentType");
            DropColumn("dbo.Posts", "Type");
        }
    }
}
