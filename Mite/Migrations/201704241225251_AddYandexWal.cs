namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddYandexWal : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "YandexWalId", c => c.String());
            DropColumn("dbo.AspNetUsers", "TimeZone");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "TimeZone", c => c.Short(nullable: false));
            DropColumn("dbo.AspNetUsers", "YandexWalId");
        }
    }
}
