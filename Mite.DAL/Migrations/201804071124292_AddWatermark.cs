namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWatermark : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Watermarks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Gravity = c.Short(nullable: false),
                        VirtualPath = c.String(),
                        ImageHash = c.String(maxLength: 160),
                        Text = c.String(),
                        FontSize = c.Int(),
                        Invert = c.Boolean(),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.ImageHash, unique: true)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Posts", "WatermarkId", c => c.Guid());
            CreateIndex("dbo.Posts", "WatermarkId");
            AddForeignKey("dbo.Posts", "WatermarkId", "dbo.Watermarks", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Posts", "WatermarkId", "dbo.Watermarks");
            DropForeignKey("dbo.Watermarks", "UserId", "dbo.Users");
            DropIndex("dbo.Watermarks", new[] { "UserId" });
            DropIndex("dbo.Watermarks", new[] { "ImageHash" });
            DropIndex("dbo.Posts", new[] { "WatermarkId" });
            DropColumn("dbo.Posts", "WatermarkId");
            DropTable("dbo.Watermarks");
        }
    }
}
