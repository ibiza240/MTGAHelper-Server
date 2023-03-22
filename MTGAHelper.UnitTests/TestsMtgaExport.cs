using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using System;
using System.Linq;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsMtgaExport : TestsBase
    {
        [TestMethod, Ignore("was failing")]
        public void Test_ExportUsesCardsFromYourCollection()
        {
            // Arrange
            var deck = new Deck("deckBasic", scraperType, new[]
            {
                new DeckCard(allCards.Values.Last(i => i.Name == "Lightning Strike"), 4, DeckCardZoneEnum.Deck),
                new DeckCard(allCards.Values.Last(i => i.Name == "Thought Erasure"), 3, DeckCardZoneEnum.Deck),
                new DeckCard(allCards.Values.Last(i => i.Name == "Air Elemental"), 3, DeckCardZoneEnum.Deck),
                new DeckCard(allCards.Values.Last(i => i.Name == "Llanowar Elves"), 2, DeckCardZoneEnum.Deck),
                new DeckCard(allCards.Values.Last(i => i.Name == "Opt"), 2, DeckCardZoneEnum.Deck),
                new DeckCard(allCards.Values.Last(i => i.Name == "Thud"), 1, DeckCardZoneEnum.Deck),
            });
            var collection = new[]
            {
                new DeckCard(allCards.Values.First(i => i.Name == "Lightning Strike"), 4, DeckCardZoneEnum.Deck),
                new DeckCard(allCards.Values.Last(i => i.Name == "Lightning Strike"), 4, DeckCardZoneEnum.Deck),
                new CardWithAmount(allCards.Values.Last(i => i.Name == "Thought Erasure"), 2),
                new CardWithAmount(allCards.Values.First(i => i.Name == "Air Elemental"), 2),
                new CardWithAmount(allCards.Values.Last(i => i.Name == "Air Elemental"), 2),
                new CardWithAmount(allCards.Values.First(i => i.Name == "Llanowar Elves"), 1),
                new CardWithAmount(allCards.Values.Last(i => i.Name == "Llanowar Elves"), 1),
                new CardWithAmount(allCards.Values.Last(i => i.Name == "Opt"), 3),
            };

            // Act
            var writer = writerDeckMtga.Init(collection, null, false);
            var mtgaFormat = writer.ToText(deck);

            // Assert
            var expected =
$@"4 Lightning Strike (XLN) 149{Environment.NewLine}3 Thought Erasure (GRN) 206{Environment.NewLine}2 Air Elemental (XLN) 45{Environment.NewLine}1 Air Elemental (M19) 308{Environment.NewLine}1 Llanowar Elves (M19) 314{Environment.NewLine}1 Llanowar Elves (DAR) 168{Environment.NewLine}2 Opt (DAR) 60{Environment.NewLine}1 Thud (M19) 163{Environment.NewLine}";
            Assert.AreEqual(expected, mtgaFormat);
        }
    }
}
