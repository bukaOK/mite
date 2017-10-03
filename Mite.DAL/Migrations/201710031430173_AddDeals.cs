namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDeals : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Deals",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Price = c.Double(),
                        Deadline = c.DateTime(),
                        CreateDate = c.DateTime(nullable: false),
                        Demands = c.String(),
                        Status = c.Short(nullable: false),
                        ImageResultSrc = c.String(maxLength: 800),
                        ImageResultSrc_50 = c.String(maxLength: 800),
                        Payed = c.Boolean(nullable: false),
                        VkReposted = c.Boolean(),
                        ServiceId = c.Guid(nullable: false),
                        ClientId = c.String(maxLength: 128),
                        AuthorId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.AuthorId)
                .ForeignKey("dbo.Users", t => t.ClientId)
                .ForeignKey("dbo.AuthorServices", t => t.ServiceId, cascadeDelete: true)
                .Index(t => t.ServiceId)
                .Index(t => t.ClientId)
                .Index(t => t.AuthorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Deals", "ServiceId", "dbo.AuthorServices");
            DropForeignKey("dbo.Deals", "ClientId", "dbo.Users");
            DropForeignKey("dbo.Deals", "AuthorId", "dbo.Users");
            DropIndex("dbo.Deals", new[] { "AuthorId" });
            DropIndex("dbo.Deals", new[] { "ClientId" });
            DropIndex("dbo.Deals", new[] { "ServiceId" });
            DropTable("dbo.Deals");
        }
    }
}
