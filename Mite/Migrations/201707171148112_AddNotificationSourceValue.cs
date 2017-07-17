namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificationSourceValue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "SourceValue", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "SourceValue");
        }
    }
}
