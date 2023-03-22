using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib;
using MTGAHelper.Lib.Analyzers.Archetypes;
using MTGAHelper.Lib.Config.Decks;
using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Server.DataAccess;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public partial class Tools : TestsBase
    {
        public Image resizeImage(int newWidth, int newHeight, Image imgPhoto)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            //Consider vertical pics
            if (sourceWidth < sourceHeight)
            {
                int buff = newWidth;

                newWidth = newHeight;
                newHeight = buff;
            }

            int sourceX = 0, sourceY = 0, destX = 0, destY = 0;
            float nPercent = 0, nPercentW = 0, nPercentH = 0;

            nPercentW = ((float)newWidth / (float)sourceWidth);
            nPercentH = ((float)newHeight / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((newWidth -
                          (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((newHeight -
                          (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Black);
            grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        [TestMethod]
        public void Tool_BuildThumbnailUrlCardArt()
        {
            var imageUrls = allCards.Values
                .Where(i => string.IsNullOrWhiteSpace(i.ImageArtUrl) == false)
                .Select(i => i.ImageArtUrl)
                .AsParallel()
                .ToArray();

            foreach (var imageArtUrl in imageUrls)
            {
                var filename = @"C:\repos\MTGAHelper\MTGAHelper.Web.UI\wwwroot\images\cardArt\thumbnail\" +
                    imageArtUrl.Split("/").Last().Split("?").First();

                if (File.Exists(filename) == false)
                {
                    using (System.Net.WebClient webClient = new System.Net.WebClient())
                    {
                        byte[] data = webClient.DownloadData(/*"https://img.scryfall.com/cards" +*/ imageArtUrl);
                        using (MemoryStream mem = new MemoryStream(data))
                        {
                            using (Image img = Image.FromStream(mem))
                            {
                                //using (var yourImage = Image.FromStream(mem))
                                //    img.Save(@"D:\repos\MTGAHelper\MTGAHelper.Web.UI\wwwroot\images\cardArt\thumbnail\" + "original.jpg", ImageFormat.Jpeg);

                                Bitmap artImageCentered = new Bitmap(img.Height, img.Height);
                                // Canvas image in the center
                                var leftOffset = (img.Width - artImageCentered.Width) / 2;
                                using (Graphics gfx = Graphics.FromImage(artImageCentered))
                                {
                                    gfx.DrawImage(img, -leftOffset, 0, img.Width, img.Height);
                                }

                                //resizedImg.Save(@"D:\repos\MTGAHelper\MTGAHelper.Web.UI\wwwroot\images\cardArt\thumbnail\" + "centered.jpg", ImageFormat.Jpeg);

                                // Resize to 32x32
                                var thumbnail = resizeImage(32, 32, artImageCentered);

                                using (MemoryStream memory = new MemoryStream())
                                {
                                    using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
                                    {
                                        thumbnail.Save(memory, ImageFormat.Jpeg);
                                        byte[] bytes = memory.ToArray();
                                        fs.Write(bytes, 0, bytes.Length);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Tool_DecksCleaning()
        {
            var configApp = provider.GetRequiredService<ConfigModelApp>();
            var manager = provider.GetRequiredService<ConfigManagerDecks>();
            var decks = manager.Get();

            var bannedCards = BannedCardsProviderTemp.GetBannedCards(BannedCardFormat.Standard);
            //var standardSets = configApp.InfoBySet.Where(i => i.Value.Formats.Contains("Standard")).Select(i => i.Key.ToLower());
            //var test = decks.Select(i => i.ScraperTypeId).Distinct().ToArray();

            var t = new DateTime(2021, 9, 1);
            var decksToKeep = decks
                //.Where(i => i.Deck.Cards.All.All(x => bannedCards.Contains(x.Card.name) == false))
                //.Where(i => i.Deck.Cards.AllExceptBasicLands.All(x => standardSets.Contains(x.Card.set.ToLower())))
                //.Where(i => (i.ScraperTypeId != "aetherhub-meta-arenastandard" && i.ScraperTypeId != "aetherhub-meta-standard") ||  i.DateCreatedUtc >= t)
                .Where(i => i.DateCreatedUtc >= t || i.ScraperTypeId.Contains("standard") == false)
                .Where(i => i.DateCreatedUtc <= DateTime.Now.AddDays(1).Date)
                //.Where(i => i.Deck.Cards.All.Any(x => bannedCards.Contains(x.Card.name)) == false)
                //.Where(i => i.Cards.All(x => x.GrpId != 73768))
                .ToArray();

            var path = Path.Combine(configApp.FolderData, "decks.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(new ConfigRootDecks { decks = decksToKeep }));
        }

        [TestMethod]
        public void Tool_DecksArchetypes()
        {
            var configApp = provider.GetRequiredService<ConfigModelApp>();
            var manager = provider.GetRequiredService<ConfigManagerDecks>();
            var decks = manager.Get();

            var archetypeIdentifier = provider.GetRequiredService<ArchetypeIdentifier>();

            foreach (var d in decks)
            {
                var archetypes = archetypeIdentifier.Identify("M20", d.CardsMainWithCommander.Select(i => i.GrpId).ToArray());

                if (archetypes.Count > 1)
                    System.Diagnostics.Debugger.Break();
                else if (archetypes.Count == 0)
                {
                    //System.Diagnostics.Debugger.Break();
                }
                else
                    d.ArchetypeId = archetypes.Single().Id;
            }

            var path = Path.Combine(configApp.FolderData, "decks.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(new ConfigRootDecks { decks = decks }));
        }

        [TestMethod]
        public void Tool_DecksKeepOnlySet()
        {
            var configApp = provider.GetRequiredService<ConfigModelApp>();
            var dictAllCards = provider.GetRequiredService<CacheSingleton<IReadOnlyDictionary<int, Card>>>().Get();
            var manager = provider.GetRequiredService<ConfigManagerDecks>();
            var decks = manager.Get();

            var sets = new[] { "M20", "WAR", "RNA" };

            var decksToKeep = decks
                .Where(i => i.CardsMainWithCommander.Any(x => sets.Contains(dictAllCards[x.GrpId].Set)))
                .ToArray();

            var path = Path.Combine(configApp.FolderData, "decks.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(new ConfigRootDecks { decks = decksToKeep }));
        }

        [TestMethod]
        public void Tool_GenerateDateFormats()
        {
            var tests = new[]
            {
                "7.6.2019 г. 6:03:48",
                "4/28/2019 6:08:26 PM",
                "31.05.2019 18.10.09",
                "31/5/2019 8:16:16 p. m.",
                "01-06-2019 08:00:48",
                "25/05/2019 7:49:41",
                "2019-05-27 2:33:10 PM",
                "31/05/2019 8:33:33 PM",
                "2019/05/27 16:33:04",
                "30-5-2019 14:23:44",
            };

            var dateFormats = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(i => i.Name.StartsWith("dz") == false)
                .Select(ci => ci.DateTimeFormat.ShortDatePattern + " " + ci.DateTimeFormat.LongTimePattern)
                .Distinct()
                .OrderBy(i => i)
                .ToArray();

            var s = JsonConvert.SerializeObject(dateFormats);

            //Dictionary<string, DateTime> dates = new Dictionary<string, DateTime>();

            //var dateNow = DateTime.Now;

            //foreach (var t in tests)
            //{
            //    var dateCleaned = t.Replace("a. m.", "AM").Replace("a. m.", "PM");
            //    var validDates = new List<DateTime>();

            //    foreach (var f in dateFormats)
            //        if (DateTime.TryParseExact(t, f, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            //            validDates.Add(dt);

            //    dates[t] = default(DateTime);
            //    var threshold = double.MaxValue;

            //    foreach (var d in validDates)
            //    {
            //        var thisThreshold = (dateNow - d).TotalSeconds;

            //        if (thisThreshold < threshold)
            //        {
            //            dates[t] = d;
            //            threshold = thisThreshold;
            //        }
            //    }
            //}
        }

        [TestMethod]
        public void Tool_GetAllCardsHash()
        {
            var allCardsJson = JsonConvert.SerializeObject(allCards);
            var hash = Fnv1aHasher.To32BitFnv1aHash(allCardsJson);
        }

        [TestMethod]
        public void Tool_GetAllInvalidJsonFiles()
        {
            var folder = @"D:\repos\MTGAHelper\BAK_PROD_20190829\data\configusers";

            var invalidFiles = new List<string>();
            foreach (var f in Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var t = JsonConvert.DeserializeObject(File.ReadAllText(f));
                }
                catch (Exception ex)
                {
                    invalidFiles.Add(f);
                }
            }

            var outputPath = @"D:\repos\MTGAHelper\invalid.json";
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(invalidFiles));
        }

        [TestMethod]
        public void Tool_GetCollectionSizeStats()
        {
            List<string> files = new List<string>();
            void DirSearch(string sDir)
            {
                try
                {
                    foreach (string d in Directory.GetDirectories(sDir))
                    {
                        foreach (string f in Directory.GetFiles(d))
                        {
                            files.Add(f);
                        }
                        DirSearch(d);
                    }
                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }
            }

            DirSearch(@"D:\repos\MTGAHelper\BAK_PROD_20190728\configusers");

            var filesByUser = files
                .Where(i => i.Contains("_Collection"))
                .OrderBy(i => i)
                .GroupBy(i => Path.GetFileNameWithoutExtension(i).Substring(0, 32))
                .Select(i => i.Last())
                .ToArray();

            var sizes = filesByUser
                .Select(i => JsonConvert.DeserializeObject<InfoByDate<Dictionary<int, int>>>(File.ReadAllText(i)))
                .Select(i => i.Info.Sum(x => x.Value))
                .OrderBy(i => i)
                .ToArray();

            var excelString = string.Join(Environment.NewLine, sizes);
        }

        [TestMethod]
        public void Tool_GetDataForDates()
        {
            var userId = "a5d0a17d7f2640198f88a21072c21bce";
            var cosmos = provider.GetRequiredService<UserDataCosmosManager>();
            cosmos.Init().Wait();

            var test = provider.GetRequiredService<UserHistoryDatesAvailable>();

            var data = test.GetDatesRecentFirst(userId).Result;
        }

        public class CustomDraftRating
        {
            [Index(0)]
            public string Set { get; set; }

            [Index(1)]
            public string Name { get; set; }

            [Index(2)]
            public int? Rating { get; set; }

            [Index(3)]
            public string Note { get; set; }
        }

        [TestMethod]
        public void Tool_UserImportCustomRatingsFromCsv()
        {
            var userId = "186fd69dbb0e40e2962c10a6c88d068f";
            var folderData = @$"C:\repos\MTGAHelper\data\configusers\{userId}";
            var ratingsFile = Path.Combine(folderData, $"{userId}_customdraftratings.json");
            var csvFile = Path.Combine(folderData, "ratings.csv");

            var ratings = JsonConvert.DeserializeObject<ICollection<CustomDraftRating>>(File.ReadAllText(ratingsFile));

            ICollection<CustomDraftRating> records = null;
            using (var reader = new CsvReader(new StreamReader(csvFile), new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
            }))
            {
                records = reader.GetRecords<CustomDraftRating>().ToArray();
            }

            foreach (var r in records)
            {
                var found = ratings.Where(i => i.Set == r.Set && i.Name == r.Name).FirstOrDefault();
                if (found != default)
                    ratings.Remove(found);

                ratings.Add(r);
            }

            File.Move(ratingsFile, $"{ratingsFile}.bak");
            File.WriteAllText(ratingsFile, JsonConvert.SerializeObject(ratings));
        }
    }
}