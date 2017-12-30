namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateChats : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ChatMembers", newName: "ChatUsers");
            CreateTable(
                "dbo.ChatUserMembers",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        ChatId = c.Guid(nullable: false),
                        Status = c.Short(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.ChatId })
                .ForeignKey("dbo.Chats", t => t.ChatId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ChatId);
            
            AddColumn("dbo.Chats", "Name", c => c.String());
            AddColumn("dbo.Chats", "ImageSrc", c => c.String());
            AddColumn("dbo.Chats", "ImageSrcCompressed", c => c.String());
            AddColumn("dbo.Chats", "Type", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatUserMembers", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatUserMembers", "ChatId", "dbo.Chats");
            DropIndex("dbo.ChatUserMembers", new[] { "ChatId" });
            DropIndex("dbo.ChatUserMembers", new[] { "UserId" });
            DropColumn("dbo.Chats", "Type");
            DropColumn("dbo.Chats", "ImageSrcCompressed");
            DropColumn("dbo.Chats", "ImageSrc");
            DropColumn("dbo.Chats", "Name");
            DropTable("dbo.ChatUserMembers");
            RenameTable(name: "dbo.ChatUsers", newName: "ChatMembers");
        }
    }
}
