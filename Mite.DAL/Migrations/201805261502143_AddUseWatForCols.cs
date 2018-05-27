namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUseWatForCols : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "UseWatermarkForCols", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "ContentLimit", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "ContentLimit");
            DropColumn("dbo.Posts", "UseWatermarkForCols");
        }
    }
}
