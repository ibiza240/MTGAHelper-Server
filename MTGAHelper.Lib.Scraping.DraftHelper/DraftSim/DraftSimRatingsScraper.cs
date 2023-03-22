//using MTGAHelper.Entity;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net.Http;
//using System.Text.RegularExpressions;

//namespace MTGAHelper.Lib.Scraping.DraftHelper.DraftSim
//{
//    public partial class DraftSimRatingsScraper
//    {
//        string[] sets = new string[] { "THB", "ELD", "M20", "WAR", "RNA", "GRN", "DOM" };
//        string[] colors = new string[] { "W", "U", "B", "R", "G" };

//        Dictionary<string, ICollection<string>> jsFilesPerSet = new Dictionary<string, ICollection<string>>
//        {
//            { "THB", new [] { "THB", "THB_land" } },
//            { "ELD", new [] { "ELD"} },
//            { "M20", new [] { "M20", "M20_land" } },
//            { "WAR", new [] { "WAR", "WAR_planeswalker" } },
//            { "RNA", new [] { "RNA", "RNA_Orzhov", "RNA_Rakdos", "RNA_Azorius", "RNA_Gruul", "RNA_Simic", "RNA_land", } },
//            { "GRN", new [] { "GRN", "GRN_Boros", "GRN_Dimir", "GRN_Golgari", "GRN_Izzet", "GRN_Selesnya", "GRN_land", } },
//            { "M19", new [] { "M19", "M19_land" } },
//            { "DOM", new [] { "DOM", "DOM_legend" } },
//            { "RIX", new [] { "RIX" } },
//            { "XLN", new [] { "XLN" } },
//        };

//        public class UrlToScrapeModel
//        {
//            public const string UrlTemplate = "https://draftsim.com/generated/{0}.js";

//            public string UrlPartSet { get; set; }

//            public UrlToScrapeModel(string urlPart)
//            {
//                UrlPartSet = urlPart;
//            }
//        }

//        ICollection<Card> allCards;

//        public DraftSimRatingsScraper(ICollection<Card> allCards)
//        {
//            this.allCards = allCards;
//        }

//        public DraftRatings Scrape(string setFilter = "")
//        {
//            var ret = new DraftRatings();
//            var draftSimCardList = new List<DraftSimCard>();

//            foreach (var set in sets.Where(i => setFilter == "" || i == setFilter))
//            {
//                var draftRatings = new List<DraftRating>();

//                foreach (var fileTemplate in jsFilesPerSet[set])
//                {
//                    var urlFormatted = string.Format(UrlToScrapeModel.UrlTemplate, fileTemplate);
//                    HttpClient client = new HttpClient();
//                    var response = client.GetAsync(urlFormatted).Result;
//                    if (response.IsSuccessStatusCode)
//                    {
//                        string jsonData = response.Content.ReadAsStringAsync().Result;
//                        jsonData = jsonData.Trim();
//                        try
//                        {
//                            jsonData = jsonData.Substring(jsonData.IndexOf("["));  //this comes through as a javascript assignment.  remove anything prior to the start of the json array
//                        }
//                        catch
//                        {
//                            System.Diagnostics.Debugger.Break();
//                        }

//                        if (jsonData.EndsWith(";"))  //get rid of the ending javascript ; also
//                        {
//                            jsonData = jsonData.Remove(jsonData.Length - 1);
//                        }
//                        draftSimCardList = JsonConvert.DeserializeObject<List<DraftSimCard>>(jsonData)
//                            .Where(i => i.name.StartsWith("Plains_") == false)
//                            .Where(i => i.name.StartsWith("Island_") == false)
//                            .Where(i => i.name.StartsWith("Swamp_") == false)
//                            .Where(i => i.name.StartsWith("Mountain_") == false)
//                            .Where(i => i.name.StartsWith("Forest_") == false)
//                            .ToList();

//                        foreach (var card in draftSimCardList)
//                        {
//                            var cardName = card.name.Replace("_", " ");

//                            // For Orzhov Guildgate 1, etc.
//                            if (Regex.Match(cardName, "i*? \\d").Success)
//                            {
//                                cardName = cardName.Substring(0, cardName.Length - 2);
//                            }

//                            if (allCards.Any(i => i.name == cardName) == false)
//                            {
//                                var cardFound = allCards.FirstOrDefault(i => i.name.Replace("// ", "") == cardName);
//                                if (cardFound != null)
//                                    cardName = cardFound.name;
//                                else
//                                {
//                                    Console.WriteLine($"ACK.  {card.name} not found in list");
//                                    continue;
//                                }
//                            }

//                            if (draftRatings.Any(i => i.CardName == cardName) == false)
//                                draftRatings.Add(new DraftRating { CardName = cardName, Rating = card.myrating, Description = "" });
//                        }
//                    }
//                }

//                var setinfo = new DraftRatingScraperResultForSet { Ratings = draftRatings };

//                for (int colorIndex = 0; colorIndex < colors.Length; colorIndex++)
//                {
//                    var top5Colors = draftRatings
//                        .Where(i =>
//                        {
//                            var c = allCards.First(x => x.name == i.CardName);
//                            return c.rarity == "Common" && c.colors.Count == 1 && c.colors.ElementAt(0) == colors[colorIndex];
//                        })
//                        .OrderByDescending(i => i.Rating)
//                        .Take(5)
//                        .Select((i, idx) => new DraftRatingTopCard(idx + 1, i.CardName))
//                        .ToArray();

//                    setinfo.TopCommonCardsByColor.Add(colors[colorIndex], top5Colors);
//                }

//                ret.RatingsBySet.Add(set, setinfo);
//            }

//            return ret;

//        }
//    }

//}