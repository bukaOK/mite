namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHelpers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Helpers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EditDocBtn = c.Boolean(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Helpers", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Helpers", new[] { "UserId" });
            DropTable("dbo.Helpers");
        }
    }
}
