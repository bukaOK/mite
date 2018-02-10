namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComicsItems : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ComicsItems",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        PostId = c.Guid(nullable: false),
                        Page = c.Int(nullable: false),
                        ContentSrc = c.String(),
                        ContentSrc_50 = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ComicsItems", "PostId", "dbo.Posts");
            DropIndex("dbo.ComicsItems", new[] { "PostId" });
            DropTable("dbo.ComicsItems");
        }
    }
}
