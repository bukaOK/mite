namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSocialLinks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SocialLinks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Vk = c.String(),
                        Twitter = c.String(),
                        Facebook = c.String(),
                        Dribbble = c.String(),
                        ArtStation = c.String(),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SocialLinks", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.SocialLinks", new[] { "UserId" });
            DropTable("dbo.SocialLinks");
        }
    }
}
