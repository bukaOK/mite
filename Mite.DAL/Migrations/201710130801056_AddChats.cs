namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddChats : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        SenderId = c.String(maxLength: 128),
                        ChatId = c.Guid(nullable: false),
                        Content = c.String(),
                        SendDate = c.DateTime(nullable: false),
                        IV = c.String(maxLength: 32),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Chats", t => t.ChatId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.SenderId)
                .Index(t => t.SenderId)
                .Index(t => t.ChatId);
            
            CreateTable(
                "dbo.Chats",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChatMessageUsers",
                c => new
                    {
                        MessageId = c.Guid(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Read = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.MessageId, t.UserId })
                .ForeignKey("dbo.ChatMessages", t => t.MessageId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.MessageId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ChatMembers",
                c => new
                    {
                        ChatId = c.Guid(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ChatId, t.UserId })
                .ForeignKey("dbo.Chats", t => t.ChatId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ChatId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Deals", "ChatId", c => c.Guid(nullable: false));
            CreateIndex("dbo.Deals", "ChatId");
            AddForeignKey("dbo.Deals", "ChatId", "dbo.Chats", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Deals", "ChatId", "dbo.Chats");
            DropForeignKey("dbo.ChatMessages", "SenderId", "dbo.Users");
            DropForeignKey("dbo.ChatMessageUsers", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatMessageUsers", "MessageId", "dbo.ChatMessages");
            DropForeignKey("dbo.ChatMessages", "ChatId", "dbo.Chats");
            DropForeignKey("dbo.ChatMembers", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatMembers", "ChatId", "dbo.Chats");
            DropIndex("dbo.ChatMembers", new[] { "UserId" });
            DropIndex("dbo.ChatMembers", new[] { "ChatId" });
            DropIndex("dbo.Deals", new[] { "ChatId" });
            DropIndex("dbo.ChatMessageUsers", new[] { "UserId" });
            DropIndex("dbo.ChatMessageUsers", new[] { "MessageId" });
            DropIndex("dbo.ChatMessages", new[] { "ChatId" });
            DropIndex("dbo.ChatMessages", new[] { "SenderId" });
            DropColumn("dbo.Deals", "ChatId");
            DropTable("dbo.ChatMembers");
            DropTable("dbo.ChatMessageUsers");
            DropTable("dbo.Chats");
            DropTable("dbo.ChatMessages");
        }
    }
}
