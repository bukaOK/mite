namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTariffsCharacter : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Characters",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(maxLength: 200),
                        DescriptionSrc = c.String(),
                        ImageSrc = c.String(),
                        Original = c.Boolean(nullable: false),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.CharacterFeatures",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(maxLength: 100),
                        Description = c.String(maxLength: 300),
                        CharacterId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Characters", t => t.CharacterId, cascadeDelete: true)
                .Index(t => t.CharacterId);
            
            CreateTable(
                "dbo.AuthorTariffs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(),
                        Price = c.Double(nullable: false),
                        AuthorId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.AuthorId, cascadeDelete: true)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "dbo.ClientTariffs",
                c => new
                    {
                        ClientId = c.String(nullable: false, maxLength: 128),
                        TariffId = c.Guid(nullable: false),
                        LastPayTimeUtc = c.DateTime(nullable: false),
                        PayStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ClientId, t.TariffId })
                .ForeignKey("dbo.Users", t => t.ClientId, cascadeDelete: true)
                .ForeignKey("dbo.AuthorTariffs", t => t.TariffId, cascadeDelete: true)
                .Index(t => t.ClientId)
                .Index(t => t.TariffId);
            
            CreateTable(
                "dbo.PostCharacters",
                c => new
                    {
                        PostId = c.Guid(nullable: false),
                        CharacterId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.PostId, t.CharacterId })
                .ForeignKey("dbo.Characters", t => t.CharacterId, cascadeDelete: true)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.CharacterId);
            
            CreateTable(
                "dbo.CharacterPosts",
                c => new
                    {
                        Character_Id = c.Guid(nullable: false),
                        Post_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Character_Id, t.Post_Id })
                .ForeignKey("dbo.Characters", t => t.Character_Id, cascadeDelete: true)
                .ForeignKey("dbo.Posts", t => t.Post_Id, cascadeDelete: true)
                .Index(t => t.Character_Id)
                .Index(t => t.Post_Id);
            
            AddColumn("dbo.Posts", "TariffId", c => c.Guid());
            AddColumn("dbo.ExternalServices", "GroupId", c => c.String());
            CreateIndex("dbo.Posts", "TariffId");
            AddForeignKey("dbo.Posts", "TariffId", "dbo.AuthorTariffs", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PostCharacters", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostCharacters", "CharacterId", "dbo.Characters");
            DropForeignKey("dbo.ClientTariffs", "TariffId", "dbo.AuthorTariffs");
            DropForeignKey("dbo.ClientTariffs", "ClientId", "dbo.Users");
            DropForeignKey("dbo.Posts", "TariffId", "dbo.AuthorTariffs");
            DropForeignKey("dbo.AuthorTariffs", "AuthorId", "dbo.Users");
            DropForeignKey("dbo.Characters", "UserId", "dbo.Users");
            DropForeignKey("dbo.CharacterPosts", "Post_Id", "dbo.Posts");
            DropForeignKey("dbo.CharacterPosts", "Character_Id", "dbo.Characters");
            DropForeignKey("dbo.CharacterFeatures", "CharacterId", "dbo.Characters");
            DropIndex("dbo.CharacterPosts", new[] { "Post_Id" });
            DropIndex("dbo.CharacterPosts", new[] { "Character_Id" });
            DropIndex("dbo.PostCharacters", new[] { "CharacterId" });
            DropIndex("dbo.PostCharacters", new[] { "PostId" });
            DropIndex("dbo.ClientTariffs", new[] { "TariffId" });
            DropIndex("dbo.ClientTariffs", new[] { "ClientId" });
            DropIndex("dbo.AuthorTariffs", new[] { "AuthorId" });
            DropIndex("dbo.CharacterFeatures", new[] { "CharacterId" });
            DropIndex("dbo.Characters", new[] { "UserId" });
            DropIndex("dbo.Posts", new[] { "TariffId" });
            DropColumn("dbo.ExternalServices", "GroupId");
            DropColumn("dbo.Posts", "TariffId");
            DropTable("dbo.CharacterPosts");
            DropTable("dbo.PostCharacters");
            DropTable("dbo.ClientTariffs");
            DropTable("dbo.AuthorTariffs");
            DropTable("dbo.CharacterFeatures");
            DropTable("dbo.Characters");
        }
    }
}
