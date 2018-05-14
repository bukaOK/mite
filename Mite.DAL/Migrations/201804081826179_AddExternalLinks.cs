namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalLinks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExternalLinks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Url = c.String(),
                        Confirmed = c.Boolean(nullable: false),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExternalLinks", "UserId", "dbo.Users");
            DropIndex("dbo.ExternalLinks", new[] { "UserId" });
            DropTable("dbo.ExternalLinks");
        }
    }
}
