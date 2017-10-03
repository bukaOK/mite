namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthorServiceDeadline : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorServices", "DeadlineNum", c => c.Int(nullable: false));
            AddColumn("dbo.AuthorServices", "DeadlineType", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuthorServices", "DeadlineType");
            DropColumn("dbo.AuthorServices", "DeadlineNum");
        }
    }
}
