namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TagUniqIndex : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Tags", "Name", c => c.String(maxLength: 150));
            CreateIndex("dbo.Tags", "Name", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Tags", new[] { "Name" });
            AlterColumn("dbo.Tags", "Name", c => c.String());
        }
    }
}
