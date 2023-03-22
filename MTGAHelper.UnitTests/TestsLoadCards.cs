using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib.Scraping.ScryfallScraper;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MTGAHelper.UnitTests
{
    public partial class Tools
    {
        private byte[] decompress(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        public void DownloadAssets(bool onlyRequired, string folder, string url)
        {
            using (var w = new WebClient())
            {
                // Get the version id
                var id = w.DownloadString(url);
                if (id == null) Assert.Fail();
                id = id.Split("\r\n")[0];

                // Get the manifest
                url = $@"https://assets.mtgarena.wizards.com/Manifest_{id}.mtga";
                var gzipdata = w.DownloadData(url);

                // Inside this gzip will be a JSON file
                var json = System.Text.Encoding.Default.GetString(decompress(gzipdata));

                var test = JsonConvert.DeserializeObject<RootObject>(json).Assets.GroupBy(i => i.AssetType).ToDictionary(i => i.Key, i => i.ToArray());

                var filesData = JsonConvert.DeserializeObject<RootObject>(json).Assets
                    .Where(i => i.AssetType == "Data");

                var filesToDownload = filesData
                    .Where(i => onlyRequired == false || i.Name.Contains("_loc") || i.Name.Contains("_cards"))
                    //.Where(i => i.Name.Contains("_cardart.mtga") == false)
                    .ToArray();

                foreach (var i in filesToDownload)
                {
                    url = $@"https://assets.mtgarena.wizards.com/{i.Name}";
                    gzipdata = w.DownloadData(url);

                    json = System.Text.Encoding.Default.GetString(decompress(gzipdata));

                    Directory.CreateDirectory(folder);

                    var cleanName = Regex.Replace(i.Name, @"_[0-9a-f]{32}", "");

                    var path = Path.GetFullPath(Path.Combine(folder, cleanName));
                    File.WriteAllText(path, json);
                }
            }
        }

        private void BackupFile(string folder, string filename)
        {
            if (File.Exists(Path.Combine(folder, filename)) == false)
                return;

            var backupFileName = Path.Combine(folder, $"{DateTime.Now.ToString("yyyyMMdd")}.{filename}.bak");
            if (File.Exists(backupFileName)) File.Delete(backupFileName);
            File.Move(Path.Combine(folder, filename), Path.Combine(folder, backupFileName));
        }

        //[TestMethod]
        //public void REBUILD_CARDS()
        //{
        //    var folderData = @"D:\repos\MTGAHelper\data\newcards";
        //    Directory.CreateDirectory(folderData);

        //    var endpointMtga = @"https://assets.mtgarena.wizards.com/External_2084_755459.mtga";

        //    using (var wc = new WebClient())
        //    {
        //        //BackupFile(folderData, "data_loc.mtga");
        //        //BackupFile(folderData, "data_cards.mtga");
        //        //BackupFile(folderData, "scryfall-scraped.json");
        //        BackupFile(folderData, "AllCardsCached.json");

        //        // Download scryfall_default.json
        //        //wc.DownloadFile(@"https://archive.scryfall.com/json/scryfall-default-cards.json", Path.Combine(folderData, "scryfall-default-cards.json"));

        //        // Download MTGA data_cards.mtga and data_loc.mtga files
        //        //DownloadAssets(true, folderData, endpointMtga);

        //        //// Scrape latest cards on Scryfall
        //        //var contents = JsonConvert.SerializeObject(new CardsScryfallScraper().Scrape("THB"));
        //        //File.WriteAllText(Path.Combine(folderData, "scryfall-scraped.json"), contents);

        //        // Build cards
        //        var dfp = new DataFolderPath(folderData);
        //        var builder = new AllCardsBuilder2(
        //            new ReaderMtgaDataCards2(new FileLoader(), dfp),
        //            new ReaderScryfallCards(new FileLoader(), dfp),
        //            new Lib.BannedCardsProviderTemp()
        //        ).Init(new Dictionary<string, int>
        //        {
        //            {"XLN", 279},
        //            {"RIX", 196},
        //            {"DOM", 269},
        //            {"M19", 280},
        //            {"GRN", 259},
        //            {"RNA", 259},
        //            {"WAR", 264},
        //            {"M20", 280},
        //            {"ELD", 269},
        //            {"THB", 254},
        //            {"IKO", 254}
        //        });
        //        var cards = builder.GetFullCards();
        //        File.WriteAllText(Path.Combine(folderData, "AllCardsCached2.json"), JsonConvert.SerializeObject(cards));
        //    }
        //}

        [TestMethod]
        public void Tool_Scrape_Scryfall_Cards()
        {
            var scraper = new CardsScryfallScraper();
            var cards = scraper.Scrape("ELD");

            var contents = JsonConvert.SerializeObject(cards);

            File.WriteAllText(@"D:\repos\MTGAHelper\data\test\scryfall-scraped.json", contents);
        }
    }
}