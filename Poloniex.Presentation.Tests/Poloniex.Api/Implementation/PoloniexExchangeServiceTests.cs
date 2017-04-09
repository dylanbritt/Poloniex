using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poloniex.Api.Implementation;

namespace Poloniex.Presentation.Tests.Poloniex.Api.Implementation
{
    [TestClass]
    public class PoloniexExchangeServiceTests
    {
        [TestMethod]
        public void RoundDecimalForPoloniexApiTest()
        {                                                                    //1234567890123456
            var roundLo = PoloniexExchangeService.RoundDecimalForPoloniexApi(0.1234567849999999M);
            var roundHi = PoloniexExchangeService.RoundDecimalForPoloniexApi(0.1234567899999999M);

            Assert.IsTrue(roundLo == 0.12345678M);
            Assert.IsTrue(roundHi == 0.12345679M);
        }
    }
}
