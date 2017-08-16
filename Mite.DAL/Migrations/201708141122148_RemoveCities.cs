namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveCities : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Users", "CityId", "dbo.Cities");
            DropIndex("dbo.Users", new[] { "CityId" });
            DropColumn("dbo.Users", "CityId");
            DropTable("dbo.Cities");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        Region = c.String(),
                        District = c.String(),
                        TimeZone = c.Int(),
                        Population = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "CityId", c => c.Guid());
            CreateIndex("dbo.Users", "CityId");
            AddForeignKey("dbo.Users", "CityId", "dbo.Cities", "Id");
        }
    }
}
