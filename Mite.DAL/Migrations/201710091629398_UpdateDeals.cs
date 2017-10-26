namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDeals : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorServices", "Reliability", c => c.Int(nullable: false));
            AddColumn("dbo.AuthorServices", "CreateDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Deals", "Rating", c => c.Short(nullable: false));
            AddColumn("dbo.Deals", "Feedback", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deals", "Feedback");
            DropColumn("dbo.Deals", "Rating");
            DropColumn("dbo.AuthorServices", "CreateDate");
            DropColumn("dbo.AuthorServices", "Reliability");
        }
    }
}
