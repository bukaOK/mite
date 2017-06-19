namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSocialServicesUserIdRequired : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SocialLinks", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.SocialLinks", new[] { "UserId" });
            AlterColumn("dbo.SocialLinks", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.SocialLinks", "UserId");
            AddForeignKey("dbo.SocialLinks", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SocialLinks", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.SocialLinks", new[] { "UserId" });
            AlterColumn("dbo.SocialLinks", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.SocialLinks", "UserId");
            AddForeignKey("dbo.SocialLinks", "UserId", "dbo.AspNetUsers", "Id");
        }
    }
}
