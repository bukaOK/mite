namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCharacterUniverse : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Characters", "Universe", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Characters", "Universe");
        }
    }
}
