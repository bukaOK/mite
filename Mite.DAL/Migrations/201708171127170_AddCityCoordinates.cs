namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCityCoordinates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Cities", "Longitude", c => c.Double());
            AddColumn("dbo.Cities", "Latitude", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Cities", "Latitude");
            DropColumn("dbo.Cities", "Longitude");
        }
    }
}
