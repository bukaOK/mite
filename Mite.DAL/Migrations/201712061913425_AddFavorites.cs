namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFavorites : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FavoritePosts",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        PostId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.PostId })
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.PostId);
            
            DropColumn("dbo.Posts", "IsImage");
            DropColumn("dbo.Posts", "IsPublished");
            DropColumn("dbo.Posts", "Blocked");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Posts", "Blocked", c => c.Boolean(nullable: false));
            AddColumn("dbo.Posts", "IsPublished", c => c.Boolean(nullable: false));
            AddColumn("dbo.Posts", "IsImage", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.FavoritePosts", "UserId", "dbo.Users");
            DropForeignKey("dbo.FavoritePosts", "PostId", "dbo.Posts");
            DropIndex("dbo.FavoritePosts", new[] { "PostId" });
            DropIndex("dbo.FavoritePosts", new[] { "UserId" });
            DropTable("dbo.FavoritePosts");
        }
    }
}
