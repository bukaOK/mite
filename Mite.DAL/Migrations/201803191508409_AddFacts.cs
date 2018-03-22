namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFacts : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.TagUsers", name: "Tag_Id", newName: "TagId");
            RenameColumn(table: "dbo.TagUsers", name: "User_Id", newName: "UserId");
            RenameIndex(table: "dbo.TagUsers", name: "IX_User_Id", newName: "IX_UserId");
            RenameIndex(table: "dbo.TagUsers", name: "IX_Tag_Id", newName: "IX_TagId");
            DropPrimaryKey("dbo.TagUsers");
            RenameTable(name: "dbo.TagUsers", newName: "UserTags");
            AddPrimaryKey("dbo.UserTags", new[] { "UserId", "TagId" });
            CreateTable(
                "dbo.DailyFacts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(nullable: false, maxLength: 100),
                        Content = c.String(nullable: false, maxLength: 400),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.UserTags");
            DropTable("dbo.DailyFacts");
            AddPrimaryKey("dbo.UserTags", new[] { "Tag_Id", "User_Id" });
            RenameIndex(table: "dbo.UserTags", name: "IX_TagId", newName: "IX_Tag_Id");
            RenameIndex(table: "dbo.UserTags", name: "IX_UserId", newName: "IX_User_Id");
            RenameColumn(table: "dbo.UserTags", name: "UserId", newName: "User_Id");
            RenameColumn(table: "dbo.UserTags", name: "TagId", newName: "Tag_Id");
            RenameTable(name: "dbo.UserTags", newName: "TagUsers");
        }
    }
}
