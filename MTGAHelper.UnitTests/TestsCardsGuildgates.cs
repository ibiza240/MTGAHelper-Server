using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using System.Linq;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsCardsGuildgates : TestsBase
    {
        [TestMethod]
        [TestCategory("Card_Guildgate")]
        public void Test_Basic_GuildgatesAll()
        {
            // Arrange
            var c = GetUserData(new[] {
                //new CardWithAmount(allCards.First(i => i.name == "Simic Guildgate (a)"), 1),
                //new CardWithAmount(allCards.First(i => i.name == "Simic Guildgate (b)"), 3),
                new CardWithAmount(allCards.Values.First(i => i.GrpId == 69405), 1),
                new CardWithAmount(allCards.Values.First(i => i.GrpId == 69406), 3),
            });
            var d = new Deck("deckBasic", scraperType, new[] {
                new DeckCard(allCards.Values.Last(i => i.Name == "Simic Guildgate"), 4, DeckCardZoneEnum.Deck)
            });

            // Act
            var res = Compare(c, new[] { d });

            // Assert
            Assert.AreEqual(0, res.GetModelDetails().Length);
        }

        [TestMethod]
        [TestCategory("Card_Guildgate")]
        public void Test_Basic_GuildgatesMissing()
        {
            // Arrange
            var c = GetUserData(new[] {
                //new CardWithAmount(allCards.First(i => i.name == "Simic Guildgate (a)"), 1),
                //new CardWithAmount(allCards.First(i => i.name == "Simic Guildgate (b)"), 3),
                new CardWithAmount(allCards.Values.First(i => i.GrpId == 69405), 1),
                new CardWithAmount(allCards.Values.First(i => i.GrpId == 69406), 2),
            });
            var d = new Deck("deckBasic", scraperType, new[] {
                new DeckCard(allCards.Values.First(i => i.Name == "Simic Guildgate"), 4, DeckCardZoneEnum.Deck)
            });

            // Act
            var res = Compare(c, new[] { d });

            // Assert
            Assert.AreEqual(1, res.GetModelDetails().Length);
        }
    }
}
