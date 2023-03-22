using CsvHelper;
using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Lib.CardProviders;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.Scraping.DraftHelper.MtgaZone.Html
{
    public class MtgaZoneRatingsScraperHtml : MtgaZoneRatingsScraperBase
    {
        public MtgaZoneRatingsScraperHtml(string folderData, ICardRepository allCards)
            : base(new MtgaZoneRatingsScraperBaseArgs
            {
                FolderData = folderData,
                AllCards = allCards
            })
        {
        }

        private readonly string[] sets = { "STX" };

        public DraftRatings LoadFromScrapedFile(string setFilter = "")
        {
            var ret = new DraftRatings();

            foreach (var set in sets.Where(i => setFilter == "" || i == setFilter))
            {
                var ratings = GetRatingsForSet(set);
                ret.RatingsBySet.Add(set, ratings);
            }

            return ret;
        }

        public async Task ScrapeFromWebsite(string set)
        {
            var dictAllCardsByName = allCards.Values.Where(i => i.Set == set).GroupBy(i => i.Name).ToDictionary(i => i.Key, i => i.First());

            var pagesBySet = new Dictionary<string, ICollection<string>>
            {
                ["KHM"] = new[]
                {
                    "kaldheim-limited-set-review-introduction-and-white",
                    "kaldheim-limited-set-review-blue",
                    "kaldheim-limited-set-review-black",
                    "kaldheim-limited-set-review-red",
                    "kaldheim-limited-set-review-green",
                    "kaldheim-limited-set-review-multicolored-artifacts-and-lands",
                },
            };

            var data = new List<MtgaZoneRatingsScraperHtmlModel>();
            HttpClient client = new HttpClient();

            foreach (var page in pagesBySet[set])
            {
                var urlFormatted = $"https://mtgazone.com/{page}";
                using (var response = await client.GetAsync(urlFormatted))
                {
                    using (var content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        var document = new HtmlDocument();
                        document.LoadHtml(result);

                        // Process
                        var divContent = document.DocumentNode.SelectSingleNode("//section[@id='content']//article//div[@class='entry-inner']");

                        string cardName = null;
                        string ratingDrifter = null;
                        string ratingRaszero = null;

                        var classDrifter = "has-pale-cyan-blue-background-color";
                        var classRaszero = "has-pale-pink-background-color";

                        foreach (var el in divContent.ChildNodes)
                        {
                            if (el.Name == "h2")
                            {
                                // save previous data
                                if (cardName != null && ratingDrifter != null && ratingRaszero != null && dictAllCardsByName.ContainsKey(cardName))
                                {
                                    data.Add(new MtgaZoneRatingsScraperHtmlModel
                                    {
                                        CardName = cardName,
                                        RatingDrifter = ratingDrifter,
                                        RatingRaszero = ratingRaszero,
                                    });
                                }


                                // start of new card
                                cardName = el.InnerText.Trim().Replace("’", "'").Replace("&rsquo;", "'");
                                //if (cardName.Contains("Guardian")) System.Diagnostics.Debugger.Break();

                                ratingDrifter = null;
                                ratingRaszero = null;
                            }
                            else
                            {
                                // continue loading current data
                                if (el.HasClass(classDrifter))
                                {
                                    var heading = el.SelectSingleNode(".//h3");
                                    if (heading == null)
                                        heading = el.SelectSingleNode(".//h2");

                                    ratingDrifter = heading.InnerText.Split(':').Last().Trim();
                                }
                                else if (el.HasClass(classRaszero))
                                {
                                    var heading = el.SelectSingleNode(".//h3");
                                    if (heading == null)
                                        heading = el.SelectSingleNode(".//h2");

                                    ratingRaszero = heading.InnerText.Split(':').Last().Trim();
                                }
                            }
                        }
                    }
                }
            }

            foreach (var i in PathwayLands)
            {
                data.Add(
                new MtgaZoneRatingsScraperHtmlModel
                {
                    CardName = i,
                    RatingDrifter = "C",
                    RatingRaszero = "C+"
                });
            }

            foreach (var i in SnowDualLands)
            {
                data.Add(
                new MtgaZoneRatingsScraperHtmlModel
                {
                    CardName = i,
                    RatingDrifter = i == "Rimewood Falls" ? "B-" : "C+",
                    RatingRaszero = i == "Rimewood Falls" ? "B-" : i == "Woodland Chasm" || i == "Ice Tunnel" ? "C+" : "C"
                });
            }

            foreach (var i in UncommonSacrificeCycle)
            {
                data.Add(
                new MtgaZoneRatingsScraperHtmlModel
                {
                    CardName = i,
                    RatingDrifter = "B-",
                    RatingRaszero = "B-"
                });
            }

            (var config, var file) = GetCsvConfig(set);
            using (var writer = new CsvWriter(new StreamWriter(file), config))
            {
                writer.WriteRecords(data);
            }
        }

        public ICollection<string> PathwayLands = new[]
        {
            "Barkchannel Pathway",
            "Tidechannel Pathway",
            "Blightstep Pathway",
            "Searstep Pathway",
            "Darkbore Pathway",
            "Slitherbore Pathway",
            "Hengegate Pathway",
            "Mistgate Pathway",
        };

        public ICollection<string> SnowDualLands = new[]
        {
            "Alpine Meadow",
            "Arctic Treeline",
            "Glacial Floodplain",
            "Highland Forest",
            "Ice Tunnel",
            "Rimewood Falls",
            "Snowfield Sinkhole",
            "Sulfurous Mire",
            "Volatile Fjord",
            "Woodland Chasm",
        };

        public ICollection<string> UncommonSacrificeCycle = new[]
        {
            "Axgard Armory",
            "Bretagard Stronghold",
            "Gates of Istfell",
            "Gnottvold Slumbermound",
            "Great Hall of Starnheim",
            "Immersturm Skullcairn",
            "Littjara Mirrorlake",
            "Port of Karfell",
            "Skemfar Elderhall",
            "Surtland Frostpyre",
        };
    }

    public class MtgaZoneRatingsScraperHtmlModel
    {
        public string CardName { get; set; }
        public string RatingDrifter { get; set; }
        public string RatingRaszero { get; set; }
    }
}
