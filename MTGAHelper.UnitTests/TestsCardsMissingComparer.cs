using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using System.Linq;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    [TestCategory("Must pass")]
    public class TestsCardsMissingComparer : TestsBase
    {
        readonly Deck deckBasic;
        readonly Deck deckLayoutDual;
        readonly Deck deckLayoutSplit;
        readonly Deck deckSentinel;

        readonly CardWithAmount[] collectionBasic;
        readonly CardWithAmount[] collectionNone;
        readonly CardWithAmount[] collectionLayoutDual;
        readonly CardWithAmount[] collectionLayoutSplit;

        public TestsCardsMissingComparer()
        {
            deckBasic = new Deck("deckBasic", scraperType, new[] { new DeckCard(allCards.Values.Where(i => i.Name == "Llanowar Elves").Last(), 4, DeckCardZoneEnum.Deck) });
            deckLayoutDual = new Deck("deckLayoutDual", scraperType, new[] { new DeckCard(allCards.Values.Where(i => i.Name == "Legion's Landing").First(), 4, DeckCardZoneEnum.Deck) });
            deckLayoutSplit = new Deck("deckLayoutSplit", scraperType, new[] { new DeckCard(allCards.Values.Where(i => i.Name == "Status // Statue").First(), 4, DeckCardZoneEnum.Deck) });

            deckSentinel = new Deck("deckSentinel", scraperType,
                new[] {
                    new DeckCard(allCards.Values.Where(i => i.Name == "Llanowar Elves").Skip(1).First(), 1, DeckCardZoneEnum.Deck)
                }
                .Union(deckLayoutDual.Cards.All.Select(i => new DeckCard(i.Card, i.Amount, i.Zone))
                .Union(deckLayoutSplit.Cards.All.Select(i => new DeckCard(i.Card, i.Amount, i.Zone))))
                .ToArray());

            collectionBasic = new[] {
                new CardWithAmount(allCards.Values.First(i => i.Name == "Llanowar Elves"), 4),
                new CardWithAmount(allCards.Values.First(i => i.Name == "History of Benalia"), 1),
            };

            collectionNone = new[] {
                new CardWithAmount(allCards.Values.First(i => i.Name == "History of Benalia"), 1),
            };

            collectionLayoutDual = new[] {
                new CardWithAmount(allCards.Values.First(i => i.Name == "Legion's Landing"), 4),
                new CardWithAmount(allCards.Values.First(i => i.Name == "History of Benalia"), 1),
            };

            collectionLayoutSplit = new[] {
                new CardWithAmount(allCards.Values.First(i => i.Name == "Status // Statue"), 4),
                new CardWithAmount(allCards.Values.First(i => i.Name == "History of Benalia"), 1),
            };

        }

        [TestMethod]
        [TestCategory("CardLayout_Normal")]
        public void Test_Basic_AllOwned()
        {
            // Arrange
            var c = GetUserData(collectionBasic);
            var d = deckBasic;

            // Act
            var res = Compare(c, new[] { d, deckSentinel });

            // Assert
            Assert.AreEqual(2, res.ByCard.Count(i => i.Value.NbMissing > 0));

            Assert.AreEqual(2, res.ByDeck.Count);
            Assert.AreEqual(0, res.ByDeck[deckBasic.Id].MissingWeight);
            Assert.AreEqual(500, res.ByDeck[deckSentinel.Id].MissingWeight);

            // Sentinel deck cards missing
            var s = res.ByCard["Status // Statue"];
            Assert.AreEqual(4, s.NbRequired);
            Assert.AreEqual(0, s.NbOwned);
            Assert.AreEqual(1, s.NbDecks);
            Assert.AreEqual(100, s.MissingWeight);
            Assert.AreEqual(1, s.ByDeck.Count);
            Assert.AreEqual(4, s.ByDeck[deckSentinel.Id].NbRequired);
            var l = res.ByCard["Legion's Landing"];
            Assert.AreEqual(4, l.NbRequired);
            Assert.AreEqual(0, l.NbOwned);
            Assert.AreEqual(1, l.NbDecks);
            Assert.AreEqual(400, l.MissingWeight);
            Assert.AreEqual(1, l.ByDeck.Count);
            Assert.AreEqual(4, l.ByDeck[deckSentinel.Id].NbRequired);
        }

        [TestMethod]
        [TestCategory("CardLayout_Normal")]
        public void Test_Basic_SomeOwned()
        {
            // Arrange
            collectionBasic[0] = new CardWithAmount(collectionBasic[0].Card, 1);
            var c = GetUserData(collectionBasic);
            var d = deckBasic;

            // Act
            var res = Compare(c, new[] { d, deckSentinel });

            // Assert
            Assert.AreEqual(3, res.ByCard.Count(i => i.Value.NbMissing > 0));

            var decks = res.ByDeck;
            Assert.AreEqual(2, decks.Count);
            Assert.AreEqual(15, decks[deckBasic.Id].MissingWeight);
            Assert.AreEqual(500, decks[deckSentinel.Id].MissingWeight);

            var e = res.ByCard["Llanowar Elves"];
            Assert.AreEqual(3, e.NbMissing);
            Assert.AreEqual(1, e.NbOwned);
            Assert.AreEqual(2, e.NbDecks);
            Assert.AreEqual(15, e.MissingWeight);
            Assert.AreEqual(2, e.ByDeck.Count);
            Assert.AreEqual(3, e.ByDeck[deckBasic.Id].NbMissing);

            // Sentinel deck cards missing
            var s = res.ByCard["Status // Statue"];
            Assert.AreEqual(4, s.NbMissing);
            Assert.AreEqual(0, s.NbOwned);
            Assert.AreEqual(1, s.NbDecks);
            Assert.AreEqual(100, s.MissingWeight);
            Assert.AreEqual(1, s.ByDeck.Count);
            Assert.AreEqual(4, s.ByDeck[deckSentinel.Id].NbMissing);
            var l = res.ByCard["Legion's Landing"];
            Assert.AreEqual(4, l.NbMissing);
            Assert.AreEqual(0, l.NbOwned);
            Assert.AreEqual(1, l.NbDecks);
            Assert.AreEqual(400, l.MissingWeight);
            Assert.AreEqual(1, l.ByDeck.Count);
            Assert.AreEqual(4, l.ByDeck[deckSentinel.Id].NbMissing);
        }

        [TestMethod]
        [TestCategory("CardLayout_Normal")]
        public void Test_Basic_NoneOwned()
        {
            // Arrange
            var c = GetUserData(collectionNone);
            var d = deckBasic;

            // Act
            var res = Compare(c, new[] { d, deckSentinel });

            // Assert
            Assert.AreEqual(3, res.ByCard.Count(i => i.Value.NbMissing > 0));

            var decks = res.ByDeck;
            Assert.AreEqual(2, decks.Count);
            Assert.AreEqual(20, decks[deckBasic.Id].MissingWeight);
            Assert.AreEqual(505, decks[deckSentinel.Id].MissingWeight);

            var e = res.ByCard["Llanowar Elves"];
            Assert.AreEqual(4, e.NbMissing);
            Assert.AreEqual(0, e.NbOwned);
            Assert.AreEqual(2, e.NbDecks);
            Assert.AreEqual(25, e.MissingWeight);
            Assert.AreEqual(2, e.ByDeck.Count);
            Assert.AreEqual(4, e.ByDeck[deckBasic.Id].NbMissing);
            Assert.AreEqual(1, e.ByDeck[deckSentinel.Id].NbMissing);

            // Sentinel deck cards missing
            var s = res.ByCard["Status // Statue"];
            Assert.AreEqual(4, s.NbMissing);
            Assert.AreEqual(0, s.NbOwned);
            Assert.AreEqual(1, s.NbDecks);
            Assert.AreEqual(100, s.MissingWeight);
            Assert.AreEqual(1, s.ByDeck.Count);
            Assert.AreEqual(4, s.ByDeck[deckSentinel.Id].NbMissing);
            var l = res.ByCard["Legion's Landing"];
            Assert.AreEqual(4, l.NbMissing);
            Assert.AreEqual(0, l.NbOwned);
            Assert.AreEqual(1, l.NbDecks);
            Assert.AreEqual(400, l.MissingWeight);
            Assert.AreEqual(1, l.ByDeck.Count);
            Assert.AreEqual(4, l.ByDeck[deckSentinel.Id].NbMissing);
        }

        [TestMethod]
        [TestCategory("CardLayout_DualSide")]
        public void Test_DualSide_AllOwned()
        {
            // Arrange
            var c = GetUserData(collectionLayoutDual);
            var d = deckLayoutDual;

            // Act
            var res = Compare(c, new[] { d, deckSentinel });

            // Assert
            Assert.AreEqual(2, res.ByCard.Count(i => i.Value.NbMissing > 0));

            var decks = res.ByDeck;
            Assert.AreEqual(2, decks.Count);
            Assert.AreEqual(0, decks[deckLayoutDual.Id].MissingWeight);
            Assert.AreEqual(105, decks[deckSentinel.Id].MissingWeight);

            // Sentinel deck cards missing
            var s = res.ByCard["Status // Statue"];
            Assert.AreEqual(4, s.NbMissing);
            Assert.AreEqual(0, s.NbOwned);
            Assert.AreEqual(1, s.NbDecks);
            Assert.AreEqual(100, s.MissingWeight);
            Assert.AreEqual(1, s.ByDeck.Count);
            Assert.AreEqual(4, s.ByDeck[deckSentinel.Id].NbMissing);
            var e = res.ByCard["Llanowar Elves"];
            Assert.AreEqual(1, e.NbMissing);
            Assert.AreEqual(0, e.NbOwned);
            Assert.AreEqual(1, e.NbDecks);
            Assert.AreEqual(5, e.MissingWeight);
            Assert.AreEqual(1, e.ByDeck.Count);
            Assert.AreEqual(1, e.ByDeck[deckSentinel.Id].NbMissing);
        }

        [TestMethod]
        [TestCategory("CardLayout_DualSide")]
        public void Test_DualSide_SomeOwned()
        {
            // Arrange
            collectionLayoutDual[0] = new CardWithAmount(collectionLayoutDual[0].Card, 1);
            var c = GetUserData(collectionLayoutDual);
            var d = deckLayoutDual;

            // Act
            var res = Compare(c, new[] { d, deckSentinel });

            // Assert
            Assert.AreEqual(3, res.ByCard.Count(i => i.Value.NbMissing > 0));

            var decks = res.ByDeck;
            Assert.AreEqual(2, decks.Count);
            Assert.AreEqual(300, decks[deckLayoutDual.Id].MissingWeight);
            Assert.AreEqual(405, decks[deckSentinel.Id].MissingWeight);

            var l = res.ByCard["Legion's Landing"];
            Assert.AreEqual(3, l.NbMissing);
            Assert.AreEqual(1, l.NbOwned);
            Assert.AreEqual(2, l.NbDecks);
            Assert.AreEqual(600, l.MissingWeight);
            Assert.AreEqual(2, l.ByDeck.Count);
            Assert.AreEqual(3, l.ByDeck[deckSentinel.Id].NbMissing);
            Assert.AreEqual(3, l.ByDeck[deckLayoutDual.Id].NbMissing);

            // Sentinel deck cards missing
            var s = res.ByCard["Status // Statue"];
            Assert.AreEqual(4, s.NbMissing);
            Assert.AreEqual(0, s.NbOwned);
            Assert.AreEqual(1, s.NbDecks);
            Assert.AreEqual(100, s.MissingWeight);
            Assert.AreEqual(1, s.ByDeck.Count);
            Assert.AreEqual(4, s.ByDeck[deckSentinel.Id].NbMissing);
            var e = res.ByCard["Llanowar Elves"];
            Assert.AreEqual(1, e.NbMissing);
            Assert.AreEqual(0, e.NbOwned);
            Assert.AreEqual(1, e.NbDecks);
            Assert.AreEqual(5, e.MissingWeight);
            Assert.AreEqual(1, e.ByDeck.Count);
            Assert.AreEqual(1, e.ByDeck[deckSentinel.Id].NbMissing);
        }

        [TestMethod]
        [TestCategory("CardLayout_DualSide")]
        public void Test_DualSide_NoneOwned()
        {
            // Arrange
            var c = GetUserData(collectionNone);
            var d = deckLayoutDual;

            // Act
            var res = Compare(c, new[] { d, deckSentinel });

            // Assert
            Assert.AreEqual(3, res.ByCard.Count(i => i.Value.NbMissing > 0));

            var decks = res.ByDeck;
            Assert.AreEqual(2, decks.Count);
            Assert.AreEqual(400, decks[deckLayoutDual.Id].MissingWeight);
            Assert.AreEqual(505, decks[deckSentinel.Id].MissingWeight);

            var l = res.ByCard["Legion's Landing"];
            Assert.AreEqual(4, l.NbMissing);
            Assert.AreEqual(0, l.NbOwned);
            Assert.AreEqual(2, l.NbDecks);
            Assert.AreEqual(800, l.MissingWeight);
            Assert.AreEqual(2, l.ByDeck.Count);
            Assert.AreEqual(4, l.ByDeck[deckSentinel.Id].NbMissing);
            Assert.AreEqual(4, l.ByDeck[deckLayoutDual.Id].NbMissing);

            // Sentinel deck cards missing
            var s = res.ByCard["Status // Statue"];
            Assert.AreEqual(4, s.NbMissing);
            Assert.AreEqual(0, s.NbOwned);
            Assert.AreEqual(1, s.NbDecks);
            Assert.AreEqual(100, s.MissingWeight);
            Assert.AreEqual(1, s.ByDeck.Count);
            Assert.AreEqual(4, s.ByDeck[deckSentinel.Id].NbMissing);
            var e = res.ByCard["Llanowar Elves"];
            Assert.AreEqual(1, e.NbMissing);
            Assert.AreEqual(0, e.NbOwned);
            Assert.AreEqual(1, e.NbDecks);
            Assert.AreEqual(5, e.MissingWeight);
            Assert.AreEqual(1, e.ByDeck.Count);
            Assert.AreEqual(1, e.ByDeck[deckSentinel.Id].NbMissing);
        }

        [TestMethod]
        [TestCategory("CardLayout_Split")]
        public void Test_Split_AllOwned()
        {
            // Arrange
            var c = GetUserData(collectionLayoutSplit);
            var d = deckLayoutSplit;

            // Act
            var res = Compare(c, new[] { d, deckSentinel });

            // Assert
            Assert.AreEqual(2, res.ByCard.Count(i => i.Value.NbMissing > 0));

            var decks = res.ByDeck;
            Assert.AreEqual(2, decks.Count);
            Assert.AreEqual(0, decks[deckLayoutSplit.Id].MissingWeight);
            Assert.AreEqual(405, decks[deckSentinel.Id].MissingWeight);

            // Sentinel deck cards missing
            var l = res.ByCard["Legion's Landing"];
            Assert.AreEqual(4, l.NbMissing);
            Assert.AreEqual(0, l.NbOwned);
            Assert.AreEqual(1, l.NbDecks);
            Assert.AreEqual(400, l.MissingWeight);
            Assert.AreEqual(1, l.ByDeck.Count);
            Assert.AreEqual(4, l.ByDeck[deckSentinel.Id].NbMissing);
            var e = res.ByCard["Llanowar Elves"];
            Assert.AreEqual(1, e.NbMissing);
            Assert.AreEqual(0, e.NbOwned);
            Assert.AreEqual(1, e.NbDecks);
            Assert.AreEqual(5, e.MissingWeight);
            Assert.AreEqual(1, e.ByDeck.Count);
            Assert.AreEqual(1, e.ByDeck[deckSentinel.Id].NbMissing);
        }

        [TestMethod]
        [TestCategory("CardLayout_Split")]
        public void Test_Split_SomeOwned()
        {
            // Arrange
            collectionLayoutSplit[0] = new CardWithAmount(collectionLayoutSplit[0].Card, 1);
            var c = GetUserData(collectionLayoutSplit);
            var d = deckLayoutSplit;

            // Act
            var res = Compare(c, new[] { d, deckSentinel });

            // Assert
            Assert.AreEqual(3, res.ByCard.Count(i => i.Value.NbMissing > 0));

            var decks = res.ByDeck;
            Assert.AreEqual(2, decks.Count);
            Assert.AreEqual(75, decks[deckLayoutSplit.Id].MissingWeight);
            Assert.AreEqual(480, decks[deckSentinel.Id].MissingWeight);

            var s = res.ByCard["Status // Statue"];
            Assert.AreEqual(3, s.NbMissing);
            Assert.AreEqual(1, s.NbOwned);
            Assert.AreEqual(2, s.NbDecks);
            Assert.AreEqual(150, s.MissingWeight);
            Assert.AreEqual(2, s.ByDeck.Count);
            Assert.AreEqual(3, s.ByDeck[deckSentinel.Id].NbMissing);
            Assert.AreEqual(3, s.ByDeck[deckLayoutSplit.Id].NbMissing);

            // Sentinel deck cards missing
            var l = res.ByCard["Legion's Landing"];
            Assert.AreEqual(4, l.NbMissing);
            Assert.AreEqual(0, l.NbOwned);
            Assert.AreEqual(1, l.NbDecks);
            Assert.AreEqual(400, l.MissingWeight);
            Assert.AreEqual(1, l.ByDeck.Count);
            Assert.AreEqual(4, l.ByDeck[deckSentinel.Id].NbMissing);
            var e = res.ByCard["Llanowar Elves"];
            Assert.AreEqual(1, e.NbMissing);
            Assert.AreEqual(0, e.NbOwned);
            Assert.AreEqual(1, e.NbDecks);
            Assert.AreEqual(5, e.MissingWeight);
            Assert.AreEqual(1, e.ByDeck.Count);
            Assert.AreEqual(1, e.ByDeck[deckSentinel.Id].NbMissing);
        }

        [TestMethod]
        [TestCategory("CardLayout_Split")]
        public void Test_Split_NoneOwned()
        {
            // Arrange
            var c = GetUserData(collectionNone);
            var d = deckLayoutSplit;

            // Act
            var res = Compare(c, new[] { d, deckSentinel });

            // Assert
            Assert.AreEqual(3, res.ByCard.Count(i => i.Value.NbMissing > 0));

            var decks = res.ByDeck;
            Assert.AreEqual(2, decks.Count);
            Assert.AreEqual(100, decks[deckLayoutSplit.Id].MissingWeight);
            Assert.AreEqual(505, decks[deckSentinel.Id].MissingWeight);

            var s = res.ByCard["Status // Statue"];
            Assert.AreEqual(4, s.NbMissing);
            Assert.AreEqual(0, s.NbOwned);
            Assert.AreEqual(2, s.NbDecks);
            Assert.AreEqual(200, s.MissingWeight);
            Assert.AreEqual(2, s.ByDeck.Count);
            Assert.AreEqual(4, s.ByDeck[deckSentinel.Id].NbMissing);
            Assert.AreEqual(4, s.ByDeck[deckLayoutSplit.Id].NbMissing);

            // Sentinel deck cards missing
            var l = res.ByCard["Legion's Landing"];
            Assert.AreEqual(4, l.NbMissing);
            Assert.AreEqual(0, l.NbOwned);
            Assert.AreEqual(1, l.NbDecks);
            Assert.AreEqual(400, l.MissingWeight);
            Assert.AreEqual(1, l.ByDeck.Count);
            Assert.AreEqual(4, l.ByDeck[deckSentinel.Id].NbMissing);
            var e = res.ByCard["Llanowar Elves"];
            Assert.AreEqual(1, e.NbMissing);
            Assert.AreEqual(0, e.NbOwned);
            Assert.AreEqual(1, e.NbDecks);
            Assert.AreEqual(5, e.MissingWeight);
            Assert.AreEqual(1, e.ByDeck.Count);
            Assert.AreEqual(1, e.ByDeck[deckSentinel.Id].NbMissing);
        }
    }
}
