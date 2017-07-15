namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBlockPost : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "Blocked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "Blocked");
        }
    }
}
