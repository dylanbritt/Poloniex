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
                        MovingAverageEventAction_EventActionId = c.Guid(),
                        MovingAverageEventAction_EventActionId1 = c.Guid(),
                    })
                .PrimaryKey(t => t.EventActionId)
                .ForeignKey("dbo.MovingAverageEventActions", t => t.MovingAverageEventAction_EventActionId)
                .ForeignKey("dbo.MovingAverageEventActions", t => t.MovingAverageEventAction_EventActionId1)
                .ForeignKey("dbo.Tasks", t => t.TaskId, cascadeDelete: true)
                .Index(t => t.TaskId)
                .Index(t => t.MovingAverageEventAction_EventActionId)
                .Index(t => t.MovingAverageEventAction_EventActionId1);
            
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
                        MovingAverageClosingValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LastClosingValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.MovingAverageId);
            
            CreateTable(
                "dbo.TradeSignalEventActions",
                c => new
                    {
                        EventActionId = c.Guid(nullable: false),
                        TradeSignalEventActionId = c.Guid(nullable: false, identity: true),
                        TradeSignalEventActionType = c.String(nullable: false, maxLength: 32),
                        SignalMovingAverageId = c.Guid(),
                        BaseMovingAverageId = c.Guid(),
                    })
                .PrimaryKey(t => t.EventActionId)
                .ForeignKey("dbo.MovingAverageEventActions", t => t.BaseMovingAverageId)
                .ForeignKey("dbo.EventActions", t => t.EventActionId)
                .ForeignKey("dbo.MovingAverageEventActions", t => t.SignalMovingAverageId)
                .Index(t => t.EventActionId)
                .Index(t => t.SignalMovingAverageId)
                .Index(t => t.BaseMovingAverageId);
            
            CreateTable(
                "dbo.TradeSignalOrders",
                c => new
                    {
                        TradeSignalOrderId = c.Guid(nullable: false, identity: true),
                        TradeSignalOrderType = c.String(nullable: false, maxLength: 32),
                        LastValueAtRequest = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LastValueAtProcessing = c.Decimal(precision: 18, scale: 2),
                        PlaceValueTradedAt = c.Decimal(precision: 18, scale: 2),
                        MoveValueTradedAt = c.Decimal(precision: 18, scale: 2),
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
            DropForeignKey("dbo.TradeSignalEventActions", "SignalMovingAverageId", "dbo.MovingAverageEventActions");
            DropForeignKey("dbo.TradeSignalEventActions", "EventActionId", "dbo.EventActions");
            DropForeignKey("dbo.TradeSignalEventActions", "BaseMovingAverageId", "dbo.MovingAverageEventActions");
            DropForeignKey("dbo.EventActions", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.TradeTasks", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.TaskLoops", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.GatherTasks", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.EventActions", "MovingAverageEventAction_EventActionId1", "dbo.MovingAverageEventActions");
            DropForeignKey("dbo.EventActions", "MovingAverageEventAction_EventActionId", "dbo.MovingAverageEventActions");
            DropForeignKey("dbo.MovingAverageEventActions", "EventActionId", "dbo.EventActions");
            DropIndex("dbo.TradeSignalEventActions", new[] { "BaseMovingAverageId" });
            DropIndex("dbo.TradeSignalEventActions", new[] { "SignalMovingAverageId" });
            DropIndex("dbo.TradeSignalEventActions", new[] { "EventActionId" });
            DropIndex("dbo.TradeTasks", new[] { "TaskId" });
            DropIndex("dbo.TaskLoops", new[] { "TaskId" });
            DropIndex("dbo.GatherTasks", new[] { "TaskId" });
            DropIndex("dbo.MovingAverageEventActions", new[] { "EventActionId" });
            DropIndex("dbo.EventActions", new[] { "MovingAverageEventAction_EventActionId1" });
            DropIndex("dbo.EventActions", new[] { "MovingAverageEventAction_EventActionId" });
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
