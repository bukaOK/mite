namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class delFollowerPrimaryKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Followers", "Id", "dbo.AspNetUsers");
            DropIndex("dbo.Followers", new[] { "Id" });
            DropPrimaryKey("dbo.Followers");
            AddColumn("dbo.Followers", "UserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Followers", "Id", c => c.Guid(nullable: false, identity: true));
            AddPrimaryKey("dbo.Followers", "Id");
            CreateIndex("dbo.Followers", "UserId");
            AddForeignKey("dbo.Followers", "UserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Followers", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Followers", new[] { "UserId" });
            DropPrimaryKey("dbo.Followers");
            AlterColumn("dbo.Followers", "Id", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.Followers", "UserId");
            AddPrimaryKey("dbo.Followers", "Id");
            CreateIndex("dbo.Followers", "Id");
            AddForeignKey("dbo.Followers", "Id", "dbo.AspNetUsers", "Id");
        }
    }
}
