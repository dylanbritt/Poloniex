namespace Poloniex.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CurrencyDataPoints",
                c => new
                    {
                        CurrencyDataPointId = c.Guid(nullable: false, identity: true),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                        ClosingDateTime = c.DateTime(nullable: false),
                        ClosingValue = c.Decimal(nullable: false, precision: 22, scale: 12),
                        CreatedDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CurrencyDataPointId);
            
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
                        MovingAverageEventActionId = c.Guid(nullable: false, identity: true),
                        MovingAverageType = c.String(nullable: false, maxLength: 32),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                        Interval = c.Int(nullable: false),
                        MinutesPerInterval = c.Int(nullable: false),
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
                        CreatedDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TaskId);
            
            CreateTable(
                "dbo.GatherTasks",
                c => new
                    {
                        TaskId = c.Guid(nullable: false),
                        GatherTaskId = c.Guid(nullable: false, identity: true),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
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
                        SecondsPerTick = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TaskId)
                .ForeignKey("dbo.Tasks", t => t.TaskId)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.TradeOrderEventActions",
                c => new
                    {
                        EventActionId = c.Guid(nullable: false),
                        TradeOrderEventActionId = c.Guid(nullable: false, identity: true),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.EventActionId)
                .ForeignKey("dbo.EventActions", t => t.EventActionId)
                .Index(t => t.EventActionId);
            
            CreateTable(
                "dbo.TradeSignalEventActions",
                c => new
                    {
                        EventActionId = c.Guid(nullable: false),
                        TradeSignalEventActionId = c.Guid(nullable: false, identity: true),
                        TradeSignalType = c.String(nullable: false, maxLength: 32),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                        ShorterMovingAverageInterval = c.Int(nullable: false),
                        LongerMovingAverageInterval = c.Int(nullable: false),
                        MinutesPerInterval = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventActionId)
                .ForeignKey("dbo.EventActions", t => t.EventActionId)
                .Index(t => t.EventActionId);
            
            CreateTable(
                "dbo.TradeSignalConfigurations",
                c => new
                    {
                        EventActionId = c.Guid(nullable: false),
                        TradeSignalConfigurationId = c.Guid(nullable: false, identity: true),
                        StopLossPercentageUpper = c.Decimal(nullable: false, precision: 22, scale: 12),
                        StopLossPercentageLower = c.Decimal(nullable: false, precision: 22, scale: 12),
                        IsStopLossTailing = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EventActionId)
                .ForeignKey("dbo.TradeSignalEventActions", t => t.EventActionId)
                .Index(t => t.EventActionId);
            
            CreateTable(
                "dbo.MovingAverages",
                c => new
                    {
                        MovingAverageId = c.Guid(nullable: false, identity: true),
                        MovingAverageType = c.String(nullable: false, maxLength: 32),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                        Interval = c.Int(nullable: false),
                        MinutesPerInterval = c.Int(nullable: false),
                        ClosingDateTime = c.DateTime(nullable: false),
                        MovingAverageValue = c.Decimal(nullable: false, precision: 22, scale: 12),
                        LastClosingValue = c.Decimal(nullable: false, precision: 22, scale: 12),
                    })
                .PrimaryKey(t => t.MovingAverageId);
            
            CreateTable(
                "dbo.TradeSignalOrders",
                c => new
                    {
                        TradeSignalOrderId = c.Guid(nullable: false, identity: true),
                        CurrencyPair = c.String(nullable: false, maxLength: 16),
                        TradeOrderType = c.String(nullable: false, maxLength: 32),
                        LastValueAtRequest = c.Decimal(nullable: false, precision: 22, scale: 12),
                        LastValueAtProcessing = c.Decimal(precision: 22, scale: 12),
                        PlaceValueTradedAt = c.Decimal(precision: 22, scale: 12),
                        MoveValueTradedAt = c.Decimal(precision: 22, scale: 12),
                        IsProcessed = c.Boolean(nullable: false),
                        InProgress = c.Boolean(nullable: false),
                        OrderRequestedDateTime = c.DateTime(nullable: false),
                        OrderCompletedDateTime = c.DateTime(),
                        CreatedByEventActionId = c.Guid(nullable: false),
                        ProcessedByEventActionId = c.Guid(),
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
            DropForeignKey("dbo.TradeSignalConfigurations", "EventActionId", "dbo.TradeSignalEventActions");
            DropForeignKey("dbo.TradeSignalEventActions", "EventActionId", "dbo.EventActions");
            DropForeignKey("dbo.TradeOrderEventActions", "EventActionId", "dbo.EventActions");
            DropForeignKey("dbo.EventActions", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.TaskLoops", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.GatherTasks", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.MovingAverageEventActions", "EventActionId", "dbo.EventActions");
            DropIndex("dbo.TradeSignalConfigurations", new[] { "EventActionId" });
            DropIndex("dbo.TradeSignalEventActions", new[] { "EventActionId" });
            DropIndex("dbo.TradeOrderEventActions", new[] { "EventActionId" });
            DropIndex("dbo.TaskLoops", new[] { "TaskId" });
            DropIndex("dbo.GatherTasks", new[] { "TaskId" });
            DropIndex("dbo.MovingAverageEventActions", new[] { "EventActionId" });
            DropIndex("dbo.EventActions", new[] { "TaskId" });
            DropTable("dbo.Users");
            DropTable("dbo.TradeSignalOrders");
            DropTable("dbo.MovingAverages");
            DropTable("dbo.TradeSignalConfigurations");
            DropTable("dbo.TradeSignalEventActions");
            DropTable("dbo.TradeOrderEventActions");
            DropTable("dbo.TaskLoops");
            DropTable("dbo.GatherTasks");
            DropTable("dbo.Tasks");
            DropTable("dbo.MovingAverageEventActions");
            DropTable("dbo.EventActions");
            DropTable("dbo.CurrencyDataPoints");
        }
    }
}
