using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using MTGAHelper.Entity.CollectionDecksCompare;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.CollectionDecksCompare;
using MTGAHelper.Lib.IO.Reader;
using MTGAHelper.Lib.IO.Writer.WriterDeckTypes;
using MTGAHelper.Lib.IoC;
using MTGAHelper.Lib.OutputLogParser.IoC;
using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Server.Data.CosmosDB.IoC;
using MTGAHelper.Server.Data.IoC;
using MTGAHelper.UnitTests.CsvMapping;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsBase
    {
        public CsvConfiguration ConfigCsv { get; private set; } = new(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        protected string userTest = ConfigModelUser.USER_LOCAL;

        protected static IServiceProvider provider;

        //static protected IUtilLib util;
        protected static IWriterDeckMtga writerDeckMtga;

        protected static ICardRepository allCards;
        //protected static ICollection<MtgaMappingItem> mappings;

        protected static string root;
        protected static string folderData;
        protected static string folderDecks;

        protected ScraperType scraperType = new ScraperType("");

        protected Dictionary<string, IDeck> Decks { get; set; }
        protected ICollection<CardWithAmount> Collection { get; set; }
        protected Dictionary<string, Card> Cards { get; set; }
        protected ICollection<dynamic> Expected { get; set; }
        protected ICollection<dynamic> ExpectedByDeck { get; set; }
        protected Dictionary<string, InfoCardMissingSummary[]> ExpectedMissingCardsSummary { get; set; }
        protected InfoCardInDeck[] ExpectedAllDecksCards { get; set; }

        [AssemblyInitialize]
        public static void Setup(TestContext testContext)
        {
            //var services = new ServiceCollection()
            //    .RegisterServicesLib("configapp.json", "configdecks.json", "configusers", "configdeckuserscrapers.json");

            root = Path.Combine(Directory.GetCurrentDirectory(), "../../../..");
            folderData = Path.Combine(root, @"MTGAHelper.UnitTests\Data");

            folderDecks = Path.Combine(root, "Decks");

            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(root, "MTGAHelper.UnitTests"))
                .AddJsonFile("configapp.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build()
                .Get<ConfigModelApp>();

            provider = new Container
                {
                    Options = {EnableAutoVerification = false}
                }
                .RegisterServicesServerData()
                .RegisterServicesCosmosDb()
                .RegisterConfigLib(config)
                .RegisterFileLoaders()
                .RegisterServicesLib()
                .RegisterServicesLibOutputLogParser();

            writerDeckMtga = provider.GetRequiredService<IWriterDeckMtga>();

            allCards = provider.GetRequiredService<ICardRepository>();

            provider.GetRequiredService<UserDataCosmosManager>().Init().Wait();
        }

        protected IImmutableUser GetUserData(ICollection<CardWithAmount> collection = null)
        {
            if (collection != null)
                Collection = collection;

            return new ConfigModelUser
            {
                Id = "testUser",
                // Custom weights
                Weights = new Dictionary<RarityEnum, UserWeightDto>
                {
                        {  RarityEnum.Mythic, new UserWeightDto(200, 1) },
                        {  RarityEnum.RareLand, new UserWeightDto(0, 0) },
                        {  RarityEnum.RareNonLand, new UserWeightDto(100, 1) },
                        {  RarityEnum.Uncommon, new UserWeightDto(25, 1) },
                        {  RarityEnum.Common, new UserWeightDto(5, 1) },
                        {  RarityEnum.Other, new UserWeightDto(0, 0) },
                },
                ScrapersActive = ImmutableSortedSet.Create(scraperType.Id)
            };
        }

        protected CardsMissingResult Compare(IImmutableUser userData, ICollection<IDeck> decks)
        {
            return provider.GetRequiredService<ICardsMissingComparer>().Init(userData, Collection).Compare(decks, Array.Empty<Deck>(), Array.Empty<Deck>());
        }

        protected void LoadTestData(string className, string methodName)
        {
            var folderTestData = Path.Combine(folderData, className, methodName);

            // Cards
            Cards = new Dictionary<string, Card>();
            using (var reader = new CsvReader(new StreamReader(Path.Combine(folderTestData, @"Cards.txt")), ConfigCsv))
            {
                reader.Context.RegisterClassMap<InfoCardMissingSummaryMap>();
                reader.Context.RegisterClassMap<InfoCardInDeckMap>();
                reader.Context.RegisterClassMap<DeckInputMap>();

                var records = reader.GetRecords<dynamic>();
                foreach (var r in records)
                {
                    // Note the .Last
                    Cards.Add(r.Code, allCards.Values.First(i => i.CompareNameWith(r.RealName)));
                }
            }

            // Decks
            Decks = new Dictionary<string, IDeck>();
            var decksFiles = Directory.GetFiles(Path.Combine(folderTestData));
            foreach (var filePathDeck in decksFiles.Where(i => Path.GetFileName(i).StartsWith("Deck")))
            {
                var deckName = Path.GetFileNameWithoutExtension(filePathDeck);
                Decks.Add(deckName, new ReaderDeckModel(allCards, ConfigCsv, Cards).Read(deckName, scraperType, filePathDeck));
            }

            // Collection
            using (var reader = new CsvReader(new StreamReader(Path.Combine(folderTestData, @"Collection.txt")), ConfigCsv))
            {
                var coll = new List<CardWithAmount>();
                var records = reader.GetRecords<dynamic>();
                foreach (var r in records)
                {
                    coll.Add(new CardWithAmount(Cards[r.Code], Convert.ToInt32(r.Qty)));
                }
                Collection = coll;
            }

            // Expected
            using (var reader = new CsvReader(new StreamReader(Path.Combine(folderTestData, @"Expected.txt")), ConfigCsv))
            {
                var coll = new List<CardWithAmount>();
                Expected = reader.GetRecords<dynamic>()
                    .Select(i => new
                    {
                        i.Code,
                        NbMissing = Convert.ToInt32(i.NbMissing),
                        Priority = Convert.ToInt32(i.Priority),
                    })
                    .ToArray();
            }

            // Expected by deck
            var f = Path.Combine(folderTestData, @"ExpectedByDeck.txt");
            if (File.Exists(f))
            {
                using (var reader = new CsvReader(new StreamReader(f), ConfigCsv))
                {
                    var coll = new List<CardWithAmount>();
                    ExpectedByDeck = reader.GetRecords<dynamic>()
                        .Select(i => new
                        {
                            i.Code,
                            i.Deck,
                            NbMissing = Convert.ToInt32(i.NbMissing),
                            Priority = Convert.ToInt32(i.Priority),
                        })
                        .ToArray();
                }
            }
            else if (Decks.Count == 1)
            {
                ExpectedByDeck = Expected
                    .Select(i => new
                    {
                        i.Code,
                        i.NbMissing,
                        i.Priority,
                        Deck = Decks.Single().Key
                    })
                    .ToArray();
            }
            else
            {
                throw new Exception("Missing file ExpectedByDeck.txt");
            }

            // Expected missing cards by summary
            f = Path.Combine(folderTestData, @"ExpectedMissingCardsSummary.txt");
            if (File.Exists(f))
            {
                using (var reader = new CsvReader(new StreamReader(f), ConfigCsv))
                {
                    var data = reader.GetRecords<InfoCardMissingSummary>();
                    ExpectedMissingCardsSummary = data
                        .GroupBy(i => i.Set)
                        .ToDictionary(i => i.Key, i => i.ToArray());
                }
            }

            // ExpectedAllDecksCards
            f = Path.Combine(folderTestData, @"ExpectedAllDecksCards.txt");
            if (File.Exists(f))
            {
                using (var reader = new CsvReader(new StreamReader(f), ConfigCsv))
                {
                    ExpectedAllDecksCards = reader.GetRecords<InfoCardInDeck>().ToArray();
                }
            }
        }

        protected void AssertFromExpectedFiles(CardsMissingResult res)
        {
            AssertExpected(res);
            AssertExpectedByDeck(res);
            AssertMissingCardsSummary(res);
            AssertAllDecksCards(res);

            Assert.AreEqual(Decks.Count, res.ByDeck.Count);
            Assert.AreEqual(Expected.Count(), res.ByCard.Count());
            Assert.AreEqual(Expected.Where(i => i.NbMissing > 0).Count(), res.GetModelDetails().Count());
            Assert.AreEqual(Expected.Sum(i => i.Priority), res.ByDeck.Sum(i => i.Value.MissingWeight));
            Assert.AreEqual(Expected.Sum(i => i.Priority), res.ByCard.Sum(i => i.Value.MissingWeight));
        }

        private void AssertExpected(CardsMissingResult res)
        {
            foreach (var expected in Expected)
            {
                string code = expected.Code;
                //if (Cards[code].name == "Dub") System.Diagnostics.Debugger.Break();

                if (expected.NbMissing == 0)
                {
                    // NbMissing
                    Assert.IsFalse(res.GetModelDetails().Any(i => i.CardName == Cards[code].Name));
                }

                // NbMissing
                //if (expected.NbMissing != res.ByCard[Cards[code].name].NbMissing)
                //    System.Diagnostics.Debugger.Break();

                Assert.AreEqual(expected.NbMissing, res.ByCard[Cards[code].Name].NbMissing);

                // Priority
                Assert.AreEqual(expected.Priority, res.ByCard[Cards[code].Name].MissingWeight);
            }
        }

        private void AssertExpectedByDeck(CardsMissingResult res)
        {
            foreach (var expected in ExpectedByDeck)
            {
                string code = expected.Code;
                string deck = expected.Deck;

                // NbMissing
                Assert.AreEqual(expected.NbMissing, res.ByCard[Cards[code].Name].ByDeck[Decks[deck].Id].NbMissing);

                // Priority
                Assert.AreEqual(expected.Priority, res.ByCard[Cards[code].Name].ByDeck[Decks[deck].Id].MissingWeight);
            }

            foreach (var expectedDeck in ExpectedByDeck.GroupBy(i => i.Deck))
            {
                string deck = expectedDeck.Key;

                Assert.AreEqual(expectedDeck.Sum(i => i.Priority), res.ByDeck[Decks[deck].Id].MissingWeight);
                Assert.AreEqual(expectedDeck.Sum(i => i.NbMissing), res.ByDeck[Decks[deck].Id].NbMissing);
            }
        }

        private void AssertMissingCardsSummary(CardsMissingResult res)
        {
            var test = res.ByCard.Where(i => i.Value.Card.Set == "M19" && i.Value.Card.Rarity == RarityEnum.Common).ToArray();

            // Missing cards summary
            if (ExpectedMissingCardsSummary != null)
            {
                var resSummary = res.GetModelSummary();

                Assert.AreEqual(ExpectedMissingCardsSummary.Count, resSummary.Count);
                foreach (var i in resSummary)
                {
                    foreach (var itemBySetAndRarity in i.Value)
                    {
                        var expected = ExpectedMissingCardsSummary[i.Key].Single(x => x.Rarity == itemBySetAndRarity.Rarity);
                        Assert.AreEqual(expected.NbMissing, itemBySetAndRarity.NbMissing);
                        Assert.AreEqual(expected.MissingWeight, itemBySetAndRarity.MissingWeight);
                        Assert.AreEqual(expected.Set, itemBySetAndRarity.Set);
                    }
                }
            }
        }

        private void AssertAllDecksCards(CardsMissingResult res)
        {
            // Missing cards summary
            if (ExpectedAllDecksCards != null)
            {
                var resAllDecksCards = res.GetModelMissingCardsAllDecks();

                Assert.AreEqual(ExpectedAllDecksCards.Length, resAllDecksCards.Length);
                for (int i = 0; i < resAllDecksCards.Length; i++)
                {
                    Assert.AreEqual(ExpectedAllDecksCards[i].Set, resAllDecksCards[i].Set);
                    Assert.AreEqual(ExpectedAllDecksCards[i].CardName, resAllDecksCards[i].CardName);
                    Assert.AreEqual(ExpectedAllDecksCards[i].Rarity.Split(' ')[0].ToLower(), resAllDecksCards[i].Rarity.ToLower());
                    Assert.AreEqual(ExpectedAllDecksCards[i].Type.Split(' ')[0], resAllDecksCards[i].Type.Split(' ')[0]);
                    Assert.AreEqual(ExpectedAllDecksCards[i].Deck, resAllDecksCards[i].Deck);
                    Assert.AreEqual(ExpectedAllDecksCards[i].NbMissingMain, resAllDecksCards[i].NbMissingMain);
                    Assert.AreEqual(ExpectedAllDecksCards[i].NbMissingSideboard, resAllDecksCards[i].NbMissingSideboard);
                }
            }
        }
    }
}