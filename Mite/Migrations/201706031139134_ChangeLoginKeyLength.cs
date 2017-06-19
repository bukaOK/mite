namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeLoginKeyLength : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.AspNetUserLogins");
            AlterColumn("dbo.AspNetUserLogins", "ProviderKey", c => c.String(nullable: false, maxLength: 300));
            AddPrimaryKey("dbo.AspNetUserLogins", new[] { "LoginProvider", "ProviderKey", "UserId" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.AspNetUserLogins");
            AlterColumn("dbo.AspNetUserLogins", "ProviderKey", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.AspNetUserLogins", new[] { "LoginProvider", "ProviderKey", "UserId" });
        }
    }
}
