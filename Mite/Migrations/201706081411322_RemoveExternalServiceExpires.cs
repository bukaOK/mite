namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveExternalServiceExpires : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ExternalServices", "ExpiresDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExternalServices", "ExpiresDate", c => c.DateTime());
        }
    }
}
