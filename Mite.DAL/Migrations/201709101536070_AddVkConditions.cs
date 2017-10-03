namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVkConditions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorServices", "VkRepostConditions", c => c.String(maxLength: 600));
            DropColumn("dbo.AuthorServices", "AdditionalConditions");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AuthorServices", "AdditionalConditions", c => c.String(maxLength: 600));
            DropColumn("dbo.AuthorServices", "VkRepostConditions");
        }
    }
}
