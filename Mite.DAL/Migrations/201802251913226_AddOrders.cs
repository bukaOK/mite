namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrders : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Deals", "ServiceId", "dbo.AuthorServices");
            DropIndex("dbo.Deals", new[] { "ServiceId" });
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Title = c.String(maxLength: 200),
                        Description = c.String(),
                        ImageSrc = c.String(),
                        ImageSrc_600 = c.String(),
                        Status = c.Short(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ExecuterId = c.String(maxLength: 128),
                        OrderTypeId = c.Guid(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        Price = c.Double(),
                        DeadlineNum = c.Int(nullable: false),
                        DeadlineType = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ExecuterId)
                .ForeignKey("dbo.AuthorServiceTypes", t => t.OrderTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ExecuterId)
                .Index(t => t.OrderTypeId);
            
            CreateTable(
                "dbo.OrderRequests",
                c => new
                    {
                        OrderId = c.Guid(nullable: false),
                        ExecuterId = c.String(nullable: false, maxLength: 128),
                        RequestDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.OrderId, t.ExecuterId })
                .ForeignKey("dbo.Users", t => t.ExecuterId, cascadeDelete: true)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.ExecuterId);
            
            AddColumn("dbo.Users", "InviteId", c => c.Guid());
            AddColumn("dbo.Deals", "OrderId", c => c.Guid());
            AlterColumn("dbo.Deals", "ServiceId", c => c.Guid());
            CreateIndex("dbo.Deals", "ServiceId");
            CreateIndex("dbo.Deals", "OrderId");
            AddForeignKey("dbo.Deals", "OrderId", "dbo.Orders", "Id");
            AddForeignKey("dbo.Deals", "ServiceId", "dbo.AuthorServices", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Deals", "ServiceId", "dbo.AuthorServices");
            DropForeignKey("dbo.OrderRequests", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderRequests", "ExecuterId", "dbo.Users");
            DropForeignKey("dbo.Deals", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Orders", "UserId", "dbo.Users");
            DropForeignKey("dbo.Orders", "OrderTypeId", "dbo.AuthorServiceTypes");
            DropForeignKey("dbo.Orders", "ExecuterId", "dbo.Users");
            DropIndex("dbo.OrderRequests", new[] { "ExecuterId" });
            DropIndex("dbo.OrderRequests", new[] { "OrderId" });
            DropIndex("dbo.Orders", new[] { "OrderTypeId" });
            DropIndex("dbo.Orders", new[] { "ExecuterId" });
            DropIndex("dbo.Orders", new[] { "UserId" });
            DropIndex("dbo.Deals", new[] { "OrderId" });
            DropIndex("dbo.Deals", new[] { "ServiceId" });
            AlterColumn("dbo.Deals", "ServiceId", c => c.Guid(nullable: false));
            DropColumn("dbo.Deals", "OrderId");
            DropColumn("dbo.Users", "InviteId");
            DropTable("dbo.OrderRequests");
            DropTable("dbo.Orders");
            CreateIndex("dbo.Deals", "ServiceId");
            AddForeignKey("dbo.Deals", "ServiceId", "dbo.AuthorServices", "Id", cascadeDelete: true);
        }
    }
}
