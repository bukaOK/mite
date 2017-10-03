namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthorServices : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuthorServices",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 800),
                        ImageSrc = c.String(),
                        ImageSrc_50 = c.String(),
                        Price = c.Double(),
                        Rating = c.Int(nullable: false),
                        ServiceTypeId = c.Guid(nullable: false),
                        AdditionalConditions = c.String(maxLength: 600),
                        AuthorId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.AuthorId)
                .ForeignKey("dbo.AuthorServiceTypes", t => t.ServiceTypeId, cascadeDelete: true)
                .Index(t => t.ServiceTypeId)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "dbo.AuthorServiceTypes",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(maxLength: 200),
                        Confirmed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Posts", "Content_50", c => c.String());
            AddColumn("dbo.Posts", "Cover_50", c => c.String());
            AddColumn("dbo.Ratings", "AuthorServiceId", c => c.Guid());
            CreateIndex("dbo.Ratings", "AuthorServiceId");
            AddForeignKey("dbo.Ratings", "AuthorServiceId", "dbo.AuthorServices", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AuthorServices", "ServiceTypeId", "dbo.AuthorServiceTypes");
            DropForeignKey("dbo.AuthorServices", "AuthorId", "dbo.Users");
            DropForeignKey("dbo.Ratings", "AuthorServiceId", "dbo.AuthorServices");
            DropIndex("dbo.Ratings", new[] { "AuthorServiceId" });
            DropIndex("dbo.AuthorServices", new[] { "AuthorId" });
            DropIndex("dbo.AuthorServices", new[] { "ServiceTypeId" });
            DropColumn("dbo.Ratings", "AuthorServiceId");
            DropColumn("dbo.Posts", "Cover_50");
            DropColumn("dbo.Posts", "Content_50");
            DropTable("dbo.AuthorServiceTypes");
            DropTable("dbo.AuthorServices");
        }
    }
}
