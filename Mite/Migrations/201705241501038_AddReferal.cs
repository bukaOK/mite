namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReferal : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ReferalId", c => c.String(maxLength: 128));
            CreateIndex("dbo.AspNetUsers", "ReferalId");
            AddForeignKey("dbo.AspNetUsers", "ReferalId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "ReferalId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetUsers", new[] { "ReferalId" });
            DropColumn("dbo.AspNetUsers", "ReferalId");
        }
    }
}
