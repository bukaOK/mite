namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SocialLinksUpdate : DbMigration
    {
        public override void Up()
        {
            //DropForeignKey("dbo.Followers", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.SocialLinks", "UserId", "dbo.AspNetUsers");
            //DropIndex("dbo.Followers", new[] { "User_Id" });
            DropPrimaryKey("dbo.SocialLinks");
            AddColumn("dbo.SocialLinks", "Instagram", c => c.String());
            AddPrimaryKey("dbo.SocialLinks", "UserId");
            AddForeignKey("dbo.SocialLinks", "UserId", "dbo.AspNetUsers", "Id");
            //DropColumn("dbo.Followers", "User_Id");
            DropColumn("dbo.SocialLinks", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SocialLinks", "Id", c => c.Guid(nullable: false));
            //AddColumn("dbo.Followers", "User_Id", c => c.String(maxLength: 128));
            DropForeignKey("dbo.SocialLinks", "UserId", "dbo.AspNetUsers");
            DropPrimaryKey("dbo.SocialLinks");
            DropColumn("dbo.SocialLinks", "Instagram");
            AddPrimaryKey("dbo.SocialLinks", "Id");
            //CreateIndex("dbo.Followers", "User_Id");
            AddForeignKey("dbo.SocialLinks", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            //AddForeignKey("dbo.Followers", "User_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
