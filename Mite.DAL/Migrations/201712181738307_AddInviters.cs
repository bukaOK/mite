namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInviters : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChatMembers", "InviterId", c => c.String(maxLength: 128));
            AddColumn("dbo.ChatMembers", "EnterDate", c => c.DateTime());
            AddColumn("dbo.Chats", "MaxMembersCount", c => c.Int(nullable: false, defaultValue: 100));
            AddColumn("dbo.Chats", "CreatorId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ChatMembers", "InviterId");
            CreateIndex("dbo.Chats", "CreatorId");
            AddForeignKey("dbo.Chats", "CreatorId", "dbo.Users", "Id");
            AddForeignKey("dbo.ChatMembers", "InviterId", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatMembers", "InviterId", "dbo.Users");
            DropForeignKey("dbo.Chats", "CreatorId", "dbo.Users");
            DropIndex("dbo.Chats", new[] { "CreatorId" });
            DropIndex("dbo.ChatMembers", new[] { "InviterId" });
            DropColumn("dbo.Chats", "CreatorId");
            DropColumn("dbo.Chats", "MaxMembersCount");
            DropColumn("dbo.ChatMembers", "EnterDate");
            DropColumn("dbo.ChatMembers", "InviterId");
        }
    }
}
