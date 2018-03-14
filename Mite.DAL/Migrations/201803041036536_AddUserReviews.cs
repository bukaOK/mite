namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserReviews : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ChatUsers", "ChatId", "dbo.Chats");
            DropForeignKey("dbo.ChatUsers", "UserId", "dbo.Users");
            DropIndex("dbo.ChatUsers", new[] { "ChatId" });
            DropIndex("dbo.ChatUsers", new[] { "UserId" });
            CreateTable(
                "dbo.UserReviews",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Review = c.String(),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            DropTable("dbo.ChatUsers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ChatUsers",
                c => new
                    {
                        ChatId = c.Guid(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ChatId, t.UserId });
            
            DropForeignKey("dbo.UserReviews", "UserId", "dbo.Users");
            DropIndex("dbo.UserReviews", new[] { "UserId" });
            DropTable("dbo.UserReviews");
            CreateIndex("dbo.ChatUsers", "UserId");
            CreateIndex("dbo.ChatUsers", "ChatId");
            AddForeignKey("dbo.ChatUsers", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ChatUsers", "ChatId", "dbo.Chats", "Id", cascadeDelete: true);
        }
    }
}
