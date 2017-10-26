namespace Mite.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAttachments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatMessageAttachments",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        MessageId = c.Guid(nullable: false),
                        Type = c.Int(nullable: false),
                        Name = c.String(maxLength: 150),
                        Src = c.String(maxLength: 500),
                        CompressedSrc = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChatMessages", t => t.MessageId, cascadeDelete: true)
                .Index(t => t.MessageId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatMessageAttachments", "MessageId", "dbo.ChatMessages");
            DropIndex("dbo.ChatMessageAttachments", new[] { "MessageId" });
            DropTable("dbo.ChatMessageAttachments");
        }
    }
}
