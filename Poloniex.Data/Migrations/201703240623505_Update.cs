namespace Poloniex.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CryptoCurrencyDataPoints",
                c => new
                    {
                        CryptoCurrencyDataPointId = c.Guid(nullable: false, identity: true),
                        Currency = c.String(nullable: false, maxLength: 16),
                        Interval = c.Int(nullable: false),
                        ClosingDateTime = c.DateTime(nullable: false),
                        ClosingValue = c.Decimal(nullable: false, precision: 22, scale: 12),
                        CreatedDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CryptoCurrencyDataPointId);
            
            CreateTable(
                "dbo.GatherTasks",
                c => new
                    {
                        TaskId = c.Guid(nullable: false),
                        GatherTaskId = c.Guid(nullable: false, identity: true),
                        CurrencyPair = c.String(nullable: false, maxLength: 32),
                        Interval = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TaskId)
                .ForeignKey("dbo.Tasks", t => t.TaskId)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        TaskId = c.Guid(nullable: false, identity: true),
                        TaskType = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.TaskId);
            
            CreateTable(
                "dbo.TaskLoops",
                c => new
                    {
                        TaskId = c.Guid(nullable: false),
                        TaskLoopId = c.Guid(nullable: false, identity: true),
                        LoopStatus = c.String(nullable: false),
                        LoopStartedDateTime = c.DateTime(),
                        Interval = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TaskId)
                .ForeignKey("dbo.Tasks", t => t.TaskId)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.TradeTasks",
                c => new
                    {
                        TaskId = c.Guid(nullable: false),
                        TradeTaskId = c.Guid(nullable: false, identity: true),
                        CurrencyPair = c.String(nullable: false, maxLength: 32),
                        Interval = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TaskId)
                .ForeignKey("dbo.Tasks", t => t.TaskId)
                .Index(t => t.TaskId);
            
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
            DropForeignKey("dbo.GatherTasks", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.TradeTasks", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.TaskLoops", "TaskId", "dbo.Tasks");
            DropIndex("dbo.TradeTasks", new[] { "TaskId" });
            DropIndex("dbo.TaskLoops", new[] { "TaskId" });
            DropIndex("dbo.GatherTasks", new[] { "TaskId" });
            DropTable("dbo.Users");
            DropTable("dbo.TradeTasks");
            DropTable("dbo.TaskLoops");
            DropTable("dbo.Tasks");
            DropTable("dbo.GatherTasks");
            DropTable("dbo.CryptoCurrencyDataPoints");
        }
    }
}
