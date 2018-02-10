namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddChatPrivacy : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "MailNotify", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "ChatPrivacy", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ChatPrivacy");
            DropColumn("dbo.Users", "MailNotify");
        }
    }
}
