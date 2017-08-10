namespace Mite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CashOperations",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        FromId = c.String(maxLength: 128),
                        ToId = c.String(maxLength: 128),
                        Sum = c.Double(nullable: false),
                        Date = c.DateTime(nullable: false),
                        OperationType = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.FromId)
                .ForeignKey("dbo.Users", t => t.ToId)
                .Index(t => t.FromId)
                .Index(t => t.ToId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Age = c.Short(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        RegisterDate = c.DateTime(nullable: false),
                        Description = c.String(),
                        AvatarSrc = c.String(),
                        Gender = c.Short(nullable: false),
                        CityId = c.Guid(),
                        ShowAd = c.Boolean(nullable: false),
                        Rating = c.Int(nullable: false),
                        YandexWalId = c.String(),
                        RefererId = c.String(maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                        Group_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Cities", t => t.CityId)
                .ForeignKey("dbo.Users", t => t.RefererId)
                .ForeignKey("dbo.Groups", t => t.Group_Id)
                .Index(t => t.CityId)
                .Index(t => t.RefererId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex")
                .Index(t => t.Group_Id);
            
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        Region = c.String(),
                        District = c.String(),
                        TimeZone = c.Int(),
                        Population = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Title = c.String(),
                        Content = c.String(),
                        IsImage = c.Boolean(nullable: false),
                        LastEdit = c.DateTime(nullable: false),
                        PublishDate = c.DateTime(),
                        IsPublished = c.Boolean(nullable: false),
                        Blocked = c.Boolean(nullable: false),
                        Cover = c.String(),
                        Description = c.String(),
                        Rating = c.Int(nullable: false),
                        Views = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        PublicTime = c.DateTime(nullable: false),
                        Content = c.String(),
                        Rating = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ParentCommentId = c.Guid(),
                        PostId = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comments", t => t.ParentCommentId)
                .ForeignKey("dbo.Posts", t => t.PostId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ParentCommentId)
                .Index(t => t.PostId);
            
            CreateTable(
                "dbo.Ratings",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Value = c.Short(nullable: false),
                        RateDate = c.DateTime(nullable: false),
                        PostId = c.Guid(),
                        CommentId = c.Guid(),
                        UserId = c.String(maxLength: 128),
                        OwnerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comments", t => t.CommentId)
                .ForeignKey("dbo.Users", t => t.OwnerId)
                .ForeignKey("dbo.Posts", t => t.PostId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.PostId)
                .Index(t => t.CommentId)
                .Index(t => t.UserId)
                .Index(t => t.OwnerId);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(maxLength: 150),
                        IsConfirmed = c.Boolean(nullable: false),
                        Checked = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.ExternalServices",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        AccessToken = c.String(),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Followers",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        FollowingUserId = c.String(maxLength: 128),
                        FollowTime = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.FollowingUserId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.FollowingUserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Title = c.String(),
                        Rating = c.Int(nullable: false),
                        MemberType = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Helpers",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        EditDocBtn = c.Boolean(nullable: false),
                        PublicPostsBtn = c.Boolean(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        NotificationType = c.Short(nullable: false),
                        IsNew = c.Boolean(nullable: false),
                        SourceValue = c.String(),
                        NotifyDate = c.DateTime(nullable: false),
                        UserId = c.String(maxLength: 128),
                        NotifyUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.NotifyUserId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.NotifyUserId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Sum = c.Double(nullable: false),
                        Date = c.DateTime(nullable: false),
                        OperationId = c.String(),
                        PaymentType = c.Short(nullable: false),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.SocialLinks",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        Vk = c.String(),
                        Twitter = c.String(),
                        Facebook = c.String(),
                        Dribbble = c.String(),
                        ArtStation = c.String(),
                        Instagram = c.String(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.TagPosts",
                c => new
                    {
                        Tag_Id = c.Guid(nullable: false),
                        Post_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Id, t.Post_Id })
                .ForeignKey("dbo.Tags", t => t.Tag_Id, cascadeDelete: true)
                .ForeignKey("dbo.Posts", t => t.Post_Id, cascadeDelete: true)
                .Index(t => t.Tag_Id)
                .Index(t => t.Post_Id);
            
            CreateTable(
                "dbo.TagUsers",
                c => new
                    {
                        Tag_Id = c.Guid(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Tag_Id, t.User_Id })
                .ForeignKey("dbo.Tags", t => t.Tag_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Tag_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SocialLinks", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Payments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.Users");
            DropForeignKey("dbo.Notifications", "NotifyUserId", "dbo.Users");
            DropForeignKey("dbo.Helpers", "UserId", "dbo.Users");
            DropForeignKey("dbo.Users", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.Followers", "UserId", "dbo.Users");
            DropForeignKey("dbo.Followers", "FollowingUserId", "dbo.Users");
            DropForeignKey("dbo.ExternalServices", "UserId", "dbo.Users");
            DropForeignKey("dbo.CashOperations", "ToId", "dbo.Users");
            DropForeignKey("dbo.CashOperations", "FromId", "dbo.Users");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.Users", "RefererId", "dbo.Users");
            DropForeignKey("dbo.Posts", "UserId", "dbo.Users");
            DropForeignKey("dbo.TagUsers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.TagUsers", "Tag_Id", "dbo.Tags");
            DropForeignKey("dbo.TagPosts", "Post_Id", "dbo.Posts");
            DropForeignKey("dbo.TagPosts", "Tag_Id", "dbo.Tags");
            DropForeignKey("dbo.Comments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Ratings", "UserId", "dbo.Users");
            DropForeignKey("dbo.Ratings", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Ratings", "OwnerId", "dbo.Users");
            DropForeignKey("dbo.Ratings", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.Comments", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Comments", "ParentCommentId", "dbo.Comments");
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserClaims", "UserId", "dbo.Users");
            DropForeignKey("dbo.Users", "CityId", "dbo.Cities");
            DropIndex("dbo.TagUsers", new[] { "User_Id" });
            DropIndex("dbo.TagUsers", new[] { "Tag_Id" });
            DropIndex("dbo.TagPosts", new[] { "Post_Id" });
            DropIndex("dbo.TagPosts", new[] { "Tag_Id" });
            DropIndex("dbo.SocialLinks", new[] { "UserId" });
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.Payments", new[] { "UserId" });
            DropIndex("dbo.Notifications", new[] { "NotifyUserId" });
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.Helpers", new[] { "UserId" });
            DropIndex("dbo.Followers", new[] { "UserId" });
            DropIndex("dbo.Followers", new[] { "FollowingUserId" });
            DropIndex("dbo.ExternalServices", new[] { "UserId" });
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.Tags", new[] { "Name" });
            DropIndex("dbo.Ratings", new[] { "OwnerId" });
            DropIndex("dbo.Ratings", new[] { "UserId" });
            DropIndex("dbo.Ratings", new[] { "CommentId" });
            DropIndex("dbo.Ratings", new[] { "PostId" });
            DropIndex("dbo.Comments", new[] { "PostId" });
            DropIndex("dbo.Comments", new[] { "ParentCommentId" });
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropIndex("dbo.Posts", new[] { "UserId" });
            DropIndex("dbo.UserLogins", new[] { "UserId" });
            DropIndex("dbo.UserClaims", new[] { "UserId" });
            DropIndex("dbo.Users", new[] { "Group_Id" });
            DropIndex("dbo.Users", "UserNameIndex");
            DropIndex("dbo.Users", new[] { "RefererId" });
            DropIndex("dbo.Users", new[] { "CityId" });
            DropIndex("dbo.CashOperations", new[] { "ToId" });
            DropIndex("dbo.CashOperations", new[] { "FromId" });
            DropTable("dbo.TagUsers");
            DropTable("dbo.TagPosts");
            DropTable("dbo.SocialLinks");
            DropTable("dbo.Roles");
            DropTable("dbo.Payments");
            DropTable("dbo.Notifications");
            DropTable("dbo.Helpers");
            DropTable("dbo.Groups");
            DropTable("dbo.Followers");
            DropTable("dbo.ExternalServices");
            DropTable("dbo.UserRoles");
            DropTable("dbo.Tags");
            DropTable("dbo.Ratings");
            DropTable("dbo.Comments");
            DropTable("dbo.Posts");
            DropTable("dbo.UserLogins");
            DropTable("dbo.UserClaims");
            DropTable("dbo.Cities");
            DropTable("dbo.Users");
            DropTable("dbo.CashOperations");
        }
    }
}
