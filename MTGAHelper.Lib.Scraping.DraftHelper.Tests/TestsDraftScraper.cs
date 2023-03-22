using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Services;
using MTGAHelper.Lib.Scraping.DraftHelper.Deathsie;
using MTGAHelper.Lib.Scraping.DraftHelper.InfiniteMythicEdition;
using MTGAHelper.Lib.Scraping.DraftHelper.MtgaZone.Spreadsheet;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTGAHelper.Lib.Scraping.DraftHelper.Tests
{
    [TestClass]
    public class TestsDraftScraper : TestsBase
    {
        //[TestMethod, Ignore("Tool")]
        //public async Task Tool_DraftScraper_ScrapeMtgaZone()
        //{
        //    await new MtgaZoneRatingsScraperHtml(Path.Combine(root, "data"), allCards.Values).ScrapeFromWebsite("KHM");
        //}

        [TestMethod]
        public void Tool_DraftScraper_UpdateRatings()
        {
            var basicLandIdentifier = provider.GetRequiredService<BasicLandIdentifier>();

            var filePath = Path.Combine(provider.GetRequiredService<IDataPath>().FolderData, "draftRatings.json");

            var existingFileContent = File.ReadAllText(filePath);
            var existing = JsonConvert.DeserializeObject<Dictionary<string, DraftRatings>>(existingFileContent);

            var sharedTools = new SharedTools(allCards);

            KeyValuePair<string, DraftRatings>[] ratingsToUpdate = {
                new("Deathsie", new DeathsieRatingsScraper(Path.Combine(root, "data"), sharedTools).Scrape("ONE")),
                new("Infinite Mythic Edition", new InfiniteMythicEditionRatingsScraper(Path.Combine(root, "data"), sharedTools).Scrape("ONE")),
                new("MTG Arena Zone", new MtgaZoneRatingsScraperSpreadsheet(Path.Combine(root, "data"), allCards).Scrape("ONE")),
            };

            var allRatings = existing;
            foreach (var newRatingsForScraper in ratingsToUpdate)
            {
                foreach (var set in newRatingsForScraper.Value.RatingsBySet)
                {
                    var check = allCards.Values.Where(i => i.Set == set.Key).Where(i => basicLandIdentifier.IsBasicLand(i) == false).Where(i => i.IsFoundInBooster);
                    foreach (var c in check)
                    {
                        //if (set.Value.Ratings.Any(i => i.CardName == c.name) == false)
                        //    Debugger.Break();
                    }

                    if (allRatings.ContainsKey(newRatingsForScraper.Key) == false)
                        allRatings.Add(newRatingsForScraper.Key, new DraftRatings());

                    allRatings[newRatingsForScraper.Key].RatingsBySet[set.Key] = set.Value;
                }
            }

            // Fix Mystical archive cards set
            var listMsyticalArchive = allCards.Values.Where(i => i.Set == "STA").Select(i => i.Name).ToArray();
            foreach (var setsForSource in allRatings.Values.Where(i => i.RatingsBySet.Keys.Contains("STX")))
            {
                var stx = setsForSource.RatingsBySet["STX"].Ratings;
                var sta = stx.Where(i => listMsyticalArchive.Contains(i.CardName)).ToArray();

                setsForSource.RatingsBySet["STX"].Ratings = stx.Where(i => listMsyticalArchive.Contains(i.CardName) == false).ToArray();
                setsForSource.RatingsBySet["STA"] = new DraftRatingScraperResultForSet
                {
                    Ratings = sta,
                };
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(allRatings));
        }
    }
}