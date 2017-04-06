using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poloniex.Core.Implementation;
using System;

namespace Poloniex.Presentation.Tests.Poloniex.Core.Implementation
{
    [TestClass]
    public class TradeManagerTests
    {
        [TestMethod]
        public void GetPercentToTradeTest()
        {
            var startingBalance = 1234;

            decimal usdtBalance = startingBalance;

            var firstTradePercentage = TradeManager.GetPercentToTradeTest(0.97M, 5, 0);
            var firstTradeValue = (usdtBalance * firstTradePercentage);
            usdtBalance = usdtBalance - firstTradeValue;

            var secondTradePercentage = TradeManager.GetPercentToTradeTest(0.97M, 5, 1);
            var secondTradeValue = (usdtBalance * secondTradePercentage);
            usdtBalance = usdtBalance - secondTradeValue;

            var thirdTradePercentage = TradeManager.GetPercentToTradeTest(0.97M, 5, 2);
            var thirdTradeValue = (usdtBalance * thirdTradePercentage);
            usdtBalance = usdtBalance - thirdTradeValue;

            var fourthTradePercentage = TradeManager.GetPercentToTradeTest(0.97M, 5, 3);
            var fourthTradeValue = (usdtBalance * fourthTradePercentage);
            usdtBalance = usdtBalance - fourthTradeValue;

            var fifthTradePercentage = TradeManager.GetPercentToTradeTest(0.97M, 5, 4);
            var fifthTradeValue = (usdtBalance * fifthTradePercentage);
            usdtBalance = usdtBalance - fifthTradeValue;

            Assert.IsTrue(Math.Abs(firstTradeValue - secondTradeValue) < 0.0000000001M);
            Assert.IsTrue(Math.Abs(firstTradeValue - thirdTradeValue) < 0.0000000001M);
            Assert.IsTrue(Math.Abs(firstTradeValue - fourthTradeValue) < 0.0000000001M);
            Assert.IsTrue(Math.Abs(firstTradeValue - fifthTradeValue) < 0.0000000001M);
            Assert.IsTrue(Math.Abs(usdtBalance - startingBalance * 0.03M) < 0.0000000001M);
        }
    }
}
