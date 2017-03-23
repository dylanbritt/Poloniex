namespace Poloniex.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CryptoCurrencyDataPoints",
                c => new
                    {
                        CryptoCurrencyDataPointId = c.Guid(nullable: false),
                        Interval = c.Int(nullable: false),
                        ClosingDateTime = c.DateTime(nullable: false),
                        ClosingValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.CryptoCurrencyDataPointId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 32),
                        Password = c.String(nullable: false, maxLength: 32),
                        EncryptedPassword = c.String(nullable: false, maxLength: 32),
                        Salt = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
            DropTable("dbo.CryptoCurrencyDataPoints");
        }
    }
}
