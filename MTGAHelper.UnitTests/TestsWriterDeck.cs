using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using MTGAHelper.Lib.IO.Writer.WriterDeckTypes;
using System;
using System;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsWriterDeck : TestsBase
    {
        readonly CardWithAmount[] collection =
        {
            new(allCards[68204], 1), // M19 plains 261
            new(allCards[68741], 1), // GRN plains 260
            new(allCards[70397], 1), // ELD plains 250
        };

        [TestMethod]
        public void TestWriterDecks_NoLandsPreference()
        {
            int[] landsPreference = Array.Empty<int>();

            var cardPlainsGrn = allCards[68741];
            var deck = new Deck("", null, new[]
            {
                new DeckCard(cardPlainsGrn, 4, DeckCardZoneEnum.Deck)
            });

            var converter = provider.GetRequiredService<IWriterDeckMtga>().Init(collection, landsPreference, false);
            string result = converter.ToText(deck);

            const string expected = "Deck\n4 Plains (GRN) 260";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestWriterDecks_WithLandsPreference()
        {
            int[] landsPreference = { 68204 };  // Plains M19

            var cardPlainsGrn = allCards[68741];
            var deck = new Deck("", null, new[]
            {
                new DeckCard(cardPlainsGrn, 4, DeckCardZoneEnum.Deck)
            });

            var converter = provider.GetRequiredService<IWriterDeckMtga>().Init(collection, landsPreference, false);
            string result = converter.ToText(deck);

            const string expected = "Deck\n4 Plains (M19) 261";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestWriterDecks_LandsPickAll_CorrectNumberOfLands()
        {
            int[] landsPreference = { 68204, 70397 };  // Plains M19, ELD

            var cardPlainsGrn = allCards[68741];
            var deck = new Deck("", null, new[]
            {
                new DeckCard(cardPlainsGrn, 5, DeckCardZoneEnum.Deck)
            });

            var converter = provider.GetRequiredService<IWriterDeckMtga>().Init(collection, landsPreference, true);
            string result = converter.ToText(deck);

            const string expected = "Deck\n3 Plains (M19) 261\n2 Plains (ELD) 250";
            Assert.AreEqual(expected, result);
        }
    }
}
