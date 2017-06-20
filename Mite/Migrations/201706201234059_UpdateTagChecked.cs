namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTagChecked : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tags", "Checked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tags", "Checked");
        }
    }
}
