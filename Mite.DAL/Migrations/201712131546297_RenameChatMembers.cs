namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameChatMembers : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ChatUserMembers", newName: "ChatMembers");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ChatMembers", newName: "ChatUserMembers");
        }
    }
}
