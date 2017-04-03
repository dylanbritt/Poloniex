namespace Poloniex.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CryptoCurrencyDataPoints",
                c => new
                    {
                        CryptoCurrencyDataPointId = c.Guid(nullable: false, identity: true),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                        ClosingDateTime = c.DateTime(nullable: false),
                        ClosingValue = c.Decimal(nullable: false, precision: 22, scale: 12),
                        CreatedDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CryptoCurrencyDataPointId);
            
            CreateTable(
                "dbo.EventActions",
                c => new
                    {
                        EventActionId = c.Guid(nullable: false, identity: true),
                        Priority = c.Int(nullable: false),
                        EventActionType = c.String(nullable: false, maxLength: 32),
                        EventActionStatus = c.String(nullable: false, maxLength: 32),
                        TaskId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.EventActionId)
                .ForeignKey("dbo.Tasks", t => t.TaskId, cascadeDelete: true)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.MovingAverageEventActions",
                c => new
                    {
                        EventActionId = c.Guid(nullable: false),
                        CalculateMovingAverageEventActionId = c.Guid(nullable: false, identity: true),
                        MovingAverageType = c.String(nullable: false, maxLength: 32),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                        Interval = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventActionId)
                .ForeignKey("dbo.EventActions", t => t.EventActionId)
                .Index(t => t.EventActionId);
            
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        TaskId = c.Guid(nullable: false, identity: true),
                        TaskType = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.TaskId);
            
            CreateTable(
                "dbo.GatherTasks",
                c => new
                    {
                        TaskId = c.Guid(nullable: false),
                        GatherTaskId = c.Guid(nullable: false, identity: true),
                        CurrencyPair = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.TaskId)
                .ForeignKey("dbo.Tasks", t => t.TaskId)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.TaskLoops",
                c => new
                    {
                        TaskId = c.Guid(nullable: false),
                        TaskLoopId = c.Guid(nullable: false, identity: true),
                        LoopStatus = c.String(nullable: false, maxLength: 32),
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
                    })
                .PrimaryKey(t => t.TaskId)
                .ForeignKey("dbo.Tasks", t => t.TaskId)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.MovingAverages",
                c => new
                    {
                        MovingAverageId = c.Guid(nullable: false, identity: true),
                        MovingAverageType = c.String(nullable: false, maxLength: 32),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                        Interval = c.Int(nullable: false),
                        ClosingDateTime = c.DateTime(nullable: false),
                        MovingAverageValue = c.Decimal(nullable: false, precision: 22, scale: 12),
                        LastClosingValue = c.Decimal(nullable: false, precision: 22, scale: 12),
                    })
                .PrimaryKey(t => t.MovingAverageId);
            
            CreateTable(
                "dbo.TradeSignalEventActions",
                c => new
                    {
                        EventActionId = c.Guid(nullable: false),
                        TradeSignalEventActionId = c.Guid(nullable: false, identity: true),
                        TradeSignalEventActionType = c.String(nullable: false, maxLength: 32),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                        SignalMovingAverageInterval = c.Int(nullable: false),
                        BaseMovingAverageInterval = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventActionId)
                .ForeignKey("dbo.EventActions", t => t.EventActionId)
                .Index(t => t.EventActionId);
            
            CreateTable(
                "dbo.TradeSignalOrders",
                c => new
                    {
                        TradeSignalOrderId = c.Guid(nullable: false, identity: true),
                        TradeSignalOrderType = c.String(nullable: false, maxLength: 32),
                        LastValueAtRequest = c.Decimal(nullable: false, precision: 22, scale: 12),
                        LastValueAtProcessing = c.Decimal(precision: 22, scale: 12),
                        PlaceValueTradedAt = c.Decimal(precision: 22, scale: 12),
                        MoveValueTradedAt = c.Decimal(precision: 22, scale: 12),
                        IsProcessed = c.Boolean(nullable: false),
                        InProgress = c.Boolean(nullable: false),
                        OrderRequestedDateTime = c.DateTime(nullable: false),
                        OrderCompletedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.TradeSignalOrderId);
            
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
            DropForeignKey("dbo.TradeSignalEventActions", "EventActionId", "dbo.EventActions");
            DropForeignKey("dbo.EventActions", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.TradeTasks", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.TaskLoops", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.GatherTasks", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.MovingAverageEventActions", "EventActionId", "dbo.EventActions");
            DropIndex("dbo.TradeSignalEventActions", new[] { "EventActionId" });
            DropIndex("dbo.TradeTasks", new[] { "TaskId" });
            DropIndex("dbo.TaskLoops", new[] { "TaskId" });
            DropIndex("dbo.GatherTasks", new[] { "TaskId" });
            DropIndex("dbo.MovingAverageEventActions", new[] { "EventActionId" });
            DropIndex("dbo.EventActions", new[] { "TaskId" });
            DropTable("dbo.Users");
            DropTable("dbo.TradeSignalOrders");
            DropTable("dbo.TradeSignalEventActions");
            DropTable("dbo.MovingAverages");
            DropTable("dbo.TradeTasks");
            DropTable("dbo.TaskLoops");
            DropTable("dbo.GatherTasks");
            DropTable("dbo.Tasks");
            DropTable("dbo.MovingAverageEventActions");
            DropTable("dbo.EventActions");
            DropTable("dbo.CryptoCurrencyDataPoints");
        }
    }
}
