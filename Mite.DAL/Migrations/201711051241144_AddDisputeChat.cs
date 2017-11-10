namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDisputeChat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deals", "DisputeChatId", c => c.Guid());
            AddColumn("dbo.Deals", "ModerId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Deals", "DisputeChatId");
            CreateIndex("dbo.Deals", "ModerId");
            AddForeignKey("dbo.Deals", "DisputeChatId", "dbo.Chats", "Id");
            AddForeignKey("dbo.Deals", "ModerId", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Deals", "ModerId", "dbo.Users");
            DropForeignKey("dbo.Deals", "DisputeChatId", "dbo.Chats");
            DropIndex("dbo.Deals", new[] { "ModerId" });
            DropIndex("dbo.Deals", new[] { "DisputeChatId" });
            DropColumn("dbo.Deals", "ModerId");
            DropColumn("dbo.Deals", "DisputeChatId");
        }
    }
}
