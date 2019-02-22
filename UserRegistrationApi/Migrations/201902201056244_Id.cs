namespace UserRegistrationApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Id : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.User", "ImageUrl");
            DropColumn("dbo.User", "Caption");
        }
        
        public override void Down()
        {
            AddColumn("dbo.User", "Caption", c => c.String());
            AddColumn("dbo.User", "ImageUrl", c => c.String());
        }
    }
}
