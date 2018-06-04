namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPaymentStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "Status", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payments", "Status");
        }
    }
}
