using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib;
using Microsoft.Extensions.DependencyInjection;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsDecksFinder : TestsBase
    {
        [TestMethod]
        public void TestsDecksFinderByCards()
        {
            var service = provider.GetRequiredService<DecksFinderByCards>();

            var cards = new[]
            {
                "Cragcrown Pathway",
                "Savai Triome",
                "Kroxa, Titan of Death's Hunger",
                "Korvold, Fae-Cursed King",
                "Shatterskull Smashing",
            };

            var result = service.GetDecksByCards(cards);
        }
    }
}
