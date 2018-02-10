namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBlackList : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlackListUsers",
                c => new
                    {
                        CallerId = c.String(nullable: false, maxLength: 128),
                        ListedUserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.CallerId, t.ListedUserId })
                .ForeignKey("dbo.Users", t => t.CallerId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.ListedUserId, cascadeDelete: true)
                .Index(t => t.CallerId)
                .Index(t => t.ListedUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlackListUsers", "ListedUserId", "dbo.Users");
            DropForeignKey("dbo.BlackListUsers", "CallerId", "dbo.Users");
            DropIndex("dbo.BlackListUsers", new[] { "ListedUserId" });
            DropIndex("dbo.BlackListUsers", new[] { "CallerId" });
            DropTable("dbo.BlackListUsers");
        }
    }
}
