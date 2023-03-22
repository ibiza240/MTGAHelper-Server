using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib.Scraping.DeckSources.Aetherhub;
using MTGAHelper.Lib.Scraping.DeckSources.MtgaTool;
using MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish;
using MTGAHelper.Lib.Scraping.DeckSources.MtgTop8;
using MTGAHelper.Lib.Scraping.DeckSources.Streamdecker;
using MTGAHelper.Lib.TextDeck;

namespace MTGAHelper.Lib.Scraping.DeckSources.Tests
{
    [TestClass]
    public class TestsDeckScraper : TestsBase
    {
        private string lastSet = "ZNR";

        [TestMethod]
        public void Test_StreamDecker()
        {
            var loader = provider.GetRequiredService<IDeckScraperStreamDecker>();
            var test = loader.Init("crokeyz").GetDecks();
        }

        [TestMethod]
        public void Test_Aetherhub_Tier1()
        {
            var test = provider.GetRequiredService<DeckScraperAetherhubTierOne>();
            test.Init(AetherhubListingEnum.Tier1, ScraperTypeFormatEnum.Unknown);
            var a = test.GetDecks();
        }

        [TestMethod]
        public void Test_Aetherhub_Meta()
        {
            var test = provider.GetRequiredService<DeckScraperAetherhubMetaPaper>();
            test.Init(AetherhubListingEnum.Meta, ScraperTypeFormatEnum.Standard);
            var a = test.GetDecks();
        }

        [TestMethod]
        public void Test_Aetherhub_User()
        {
            var test = provider.GetRequiredService<DeckScraperAetherhubUserDecks>();
            test.Init(AetherhubListingEnum.User, ScraperTypeFormatEnum.Standard, "aliasv");
            var a = test.GetDecks();
        }

        [TestMethod]
        public void Test_MtgTop8()
        {
            var test = provider.GetRequiredService<DeckScraperMtgTop8DecksToBeat>();
            var a = test.GetDecks();
        }

        [TestMethod]
        public void Test_MtgaTool()
        {
            var test = provider.GetRequiredService<DeckScraperMtgaTool>();
            test.Init(ScraperTypeFormatEnum.ArenaStandard);
            var a = test.GetDecks();
        }

        [TestMethod]
        public void Test_MtgGoldfish_Articles()
        {
            var loader = provider.GetRequiredService<IDeckScraperMtgGoldfishArticle>();
            var AgainstTheOdds = loader.Init(MtgGoldfishArticleEnum.AgainstTheOdds).GetDecks();
            var BudgetArena = loader.Init(MtgGoldfishArticleEnum.BudgetArena).GetDecks();
            var BudgetMagic = loader.Init(MtgGoldfishArticleEnum.BudgetMagic).GetDecks();
            var FishFiveO = loader.Init(MtgGoldfishArticleEnum.FishFiveO).GetDecks();
            var GoldfishGladiators = loader.Init(MtgGoldfishArticleEnum.GoldfishGladiators).GetDecks();
            var InstantDeckTech = loader.Init(MtgGoldfishArticleEnum.InstantDeckTech).GetDecks();
            var MuchAbrew = loader.Init(MtgGoldfishArticleEnum.MuchAbrew).GetDecks();
            var SingleScoop = loader.Init(MtgGoldfishArticleEnum.SingleScoop).GetDecks();
            var StreamHighlights = loader.Init(MtgGoldfishArticleEnum.StreamHighlights).GetDecks();
        }

        [TestMethod]
        public void Test_MtgGoldfish_Meta()
        {
            var loader2 = provider.GetRequiredService<IDeckScraperMtgGoldfishMeta>();
            var test2 = loader2.Init(ScraperTypeFormatEnum.Standard).GetDecks();
        }

        [TestMethod]
        public void Test_MtgGoldfish_Tournaments()
        {
            var loader2 = provider.GetRequiredService<DeckScraperMtgGoldfishTournament>();
            var test2 = loader2.Init(ScraperTypeFormatEnum.Standard, MtgGoldfishArticleEnum.Tournaments).GetDecks();
        }

        [TestMethod]
        public void Test_MtgAetherhubByUrl()
        {
            var test = provider.GetRequiredService<DeckScraperAetherhubByUrl>().GetDeck("https://aetherhub.com/Deck/Public/148407");
            var converter = provider.GetRequiredService<IMtgaTextDeckConverter>();
            var cards = converter.Convert(test.name, test.mtgaFormat);
            var deck = new Entity.Deck(test.name, new ScraperType(ScraperTypeEnum.UserCustomDeck, "test"), cards);
        }
    }
}