namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalService : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Followers", "FollowingUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Followers", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Followers", new[] { "FollowingUserId" });
            DropIndex("dbo.Followers", new[] { "UserId" });
            DropPrimaryKey("dbo.Followers");
            DropPrimaryKey("dbo.AspNetUserLogins");
            CreateTable(
                "dbo.ExternalServices",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        AccessToken = c.String(),
                        UserId = c.String(maxLength: 128),
                        ExpiresDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            AlterColumn("dbo.Followers", "Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.Followers", "FollowingUserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Followers", "UserId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.AspNetUserLogins", "ProviderKey", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.Followers", "Id");
            AddPrimaryKey("dbo.AspNetUserLogins", new[] { "LoginProvider", "ProviderKey", "UserId" });
            CreateIndex("dbo.Followers", "FollowingUserId");
            CreateIndex("dbo.Followers", "UserId");
            AddForeignKey("dbo.Followers", "FollowingUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Followers", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Followers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Followers", "FollowingUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ExternalServices", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.ExternalServices", new[] { "UserId" });
            DropIndex("dbo.Followers", new[] { "UserId" });
            DropIndex("dbo.Followers", new[] { "FollowingUserId" });
            DropPrimaryKey("dbo.AspNetUserLogins");
            DropPrimaryKey("dbo.Followers");
            AlterColumn("dbo.AspNetUserLogins", "ProviderKey", c => c.String(nullable: false, maxLength: 300));
            AlterColumn("dbo.Followers", "UserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Followers", "FollowingUserId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Followers", "Id", c => c.Guid(nullable: false, identity: true));
            DropTable("dbo.ExternalServices");
            AddPrimaryKey("dbo.AspNetUserLogins", new[] { "LoginProvider", "ProviderKey", "UserId" });
            AddPrimaryKey("dbo.Followers", "Id");
            CreateIndex("dbo.Followers", "UserId");
            CreateIndex("dbo.Followers", "FollowingUserId");
            AddForeignKey("dbo.Followers", "UserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Followers", "FollowingUserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
