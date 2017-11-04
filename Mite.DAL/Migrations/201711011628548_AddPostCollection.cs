namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPostCollection : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PostCollectionItems",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        PostId = c.Guid(nullable: false),
                        Description = c.String(maxLength: 300),
                        ContentSrc = c.String(),
                        ContentSrc_50 = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId);
            
            AddColumn("dbo.Users", "Reliability", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PostCollectionItems", "PostId", "dbo.Posts");
            DropIndex("dbo.PostCollectionItems", new[] { "PostId" });
            DropColumn("dbo.Users", "Reliability");
            DropTable("dbo.PostCollectionItems");
        }
    }
}
