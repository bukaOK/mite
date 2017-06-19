namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DelSuccess : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "PaymentType", c => c.Byte(nullable: false));
            DropColumn("dbo.Payments", "Succeeded");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Payments", "Succeeded", c => c.Boolean());
            DropColumn("dbo.Payments", "PaymentType");
        }
    }
}
