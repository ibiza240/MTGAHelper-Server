using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib.Scraping;
using MTGAHelper.Lib.Scraping.NewsScraper.MtgaZone;
using Newtonsoft.Json;
using System.IO;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsNewsScraper : TestsBase
    {
        [TestMethod]
        public void Test_News_MtgaZone()
        {
            var loader = provider.GetRequiredService<NewsScraperMtgaZone>();
            var test = loader.GetNews();
        }

        [TestMethod, Ignore]
        public void Test_Jumpstart_Themes_Scraper()
        {
            var scraper = new JumpstartThemesScraper();
            var test = scraper.GetPacks();

            var json = JsonConvert.SerializeObject(test);
            File.WriteAllText(@"C:\Users\BL\source\repos\MTGAHelper\data\jumpstartThemes.json", json);
        }
    }
}