using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Scraping.DraftHelper.Deathsie;
using MTGAHelper.Lib.Scraping.DraftHelper.InfiniteMythicEdition;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MTGAHelper.Lib.Scraping.DraftHelper.Tests
{
    [TestClass, Ignore("tool")]
    public class Samples
    {
        private const string FOLDER_DATA = "../../../../data";
        private const string filePathAllCardsCached = FOLDER_DATA + "/AllCardsCached2.json";
        private readonly ICardRepository allCards = new CardRepositoryFromCollection(
            JsonConvert.DeserializeObject<IReadOnlyCollection<Card>>(File.ReadAllText(filePathAllCardsCached))!);

        //[TestMethod]
        //public void SampleChannelFireball()
        //{
        //    var scraper = new ChannelFireballLsvDraftRatingsScraper2(allCards);
        //    var results = scraper.Scrape("THB");
        //}

        //[TestMethod]
        //public void SampleDraftSim()
        //{
        //    // https://draftsim.com/
        //    var scraper = new DraftSimRatingsScraper(allCards);
        //    var results = scraper.Scrape("DOM");
        //    //var test = results.RatingsBySet["GRN"].Ratings.GroupBy(i => i.CardName).Where(i => i.Count() > 1).ToArray();
        //    //var test2 = test.Where(i => i.Key.StartsWith("Crackling"));
        //}

        [TestMethod]
        public void SampleFrankKarsten()
        {
            // https://docs.google.com/spreadsheets/d/e/2PACX-1vSX3Jjurk3DJmYrlufl_U8my9A0iiXZLwIm4O7Li1e2REwcZJSR5DXAQhgamCi60CsYpmBWNUsYCbjJ/pubhtml?gid=0
        }

        [TestMethod]
        public void SampleDeathsie()
        {
            var scraper = new DeathsieRatingsScraper(FOLDER_DATA, new SharedTools(allCards));
            var results = scraper.Scrape("");
        }

        [TestMethod]
        public void SampleInfiniteMythicEdition()
        {
            var scraper = new InfiniteMythicEditionRatingsScraper(FOLDER_DATA, new SharedTools(allCards));
            var results = scraper.Scrape("");
        }

        //[TestMethod]
        //public void SampleDraftaholicsAnonymous()
        //{
        //    // http://www.draftaholicsanonymous.com/
        //}

        //[TestMethod]
        //public void SampleCommunityReview()
        //{
        //    var scraper = new CommunityReviewScraper(allCards);
        //    var results = scraper.Scrape("");
        //}

        [TestMethod]
        public void Sample17Lands()
        {
            // https://www.17lands.com/
        }
    }
}