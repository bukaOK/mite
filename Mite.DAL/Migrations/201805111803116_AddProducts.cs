namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProducts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Price = c.Double(nullable: false),
                        BonusPath = c.String(),
                        BonusDescription = c.String(),
                        ForAuthors = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Purchases",
                c => new
                    {
                        ProductId = c.Guid(nullable: false),
                        BuyerId = c.String(nullable: false, maxLength: 128),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.ProductId, t.BuyerId })
                .ForeignKey("dbo.Users", t => t.BuyerId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.BuyerId);
            
            AddColumn("dbo.Posts", "ProductId", c => c.Guid());
            CreateIndex("dbo.Posts", "ProductId");
            AddForeignKey("dbo.Posts", "ProductId", "dbo.Products", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Posts", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Purchases", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Purchases", "BuyerId", "dbo.Users");
            DropIndex("dbo.Purchases", new[] { "BuyerId" });
            DropIndex("dbo.Purchases", new[] { "ProductId" });
            DropIndex("dbo.Posts", new[] { "ProductId" });
            DropColumn("dbo.Posts", "ProductId");
            DropTable("dbo.Purchases");
            DropTable("dbo.Products");
        }
    }
}
