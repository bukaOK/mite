namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCash : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.AspNetUsers", name: "ReferalId", newName: "RefererId");
            RenameIndex(table: "dbo.AspNetUsers", name: "IX_ReferalId", newName: "IX_RefererId");
            CreateTable(
                "dbo.CashOperations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FromId = c.String(maxLength: 128),
                        ToId = c.String(maxLength: 128),
                        Sum = c.Double(nullable: false),
                        Date = c.DateTime(nullable: false),
                        OperationType = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.FromId)
                .ForeignKey("dbo.AspNetUsers", t => t.ToId)
                .Index(t => t.FromId)
                .Index(t => t.ToId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Sum = c.Double(nullable: false),
                        Succeeded = c.Boolean(),
                        Date = c.DateTime(nullable: false),
                        OperationId = c.String(),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CashOperations", "ToId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CashOperations", "FromId", "dbo.AspNetUsers");
            DropIndex("dbo.Payments", new[] { "UserId" });
            DropIndex("dbo.CashOperations", new[] { "ToId" });
            DropIndex("dbo.CashOperations", new[] { "FromId" });
            DropTable("dbo.Payments");
            DropTable("dbo.CashOperations");
            RenameIndex(table: "dbo.AspNetUsers", name: "IX_RefererId", newName: "IX_ReferalId");
            RenameColumn(table: "dbo.AspNetUsers", name: "RefererId", newName: "ReferalId");
        }
    }
}
