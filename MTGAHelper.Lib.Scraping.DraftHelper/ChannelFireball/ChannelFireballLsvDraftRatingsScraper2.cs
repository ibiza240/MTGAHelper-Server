﻿//using HtmlAgilityPack;
//using MTGAHelper.Entity;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;

//namespace MTGAHelper.Lib.Scraping.DraftHelper.ChannelFireball
//{
//    public class ChannelFireballLsvDraftRatingsScraper2
//    {
//        ICollection<Card> allCards;
//        ManualData manualData = new ManualData();

//        public ChannelFireballLsvDraftRatingsScraper2(ICollection<Card> allCards)
//        {
//            this.allCards = allCards;
//        }

//        public DraftRatings Scrape(string setFilter = "")
//        {
//            var allCardsNames = allCards.Select(i => i.name).ToArray();
//            var ret = new DraftRatings();

//            foreach (var set in manualData.modelBySet.Where(i => setFilter == "" || i.Key == setFilter))
//            {
//                var setInfo = new DraftRatingScraperResultForSet();
//                foreach (var c in set.Value.DictUrlPartColor)
//                {
//                    var draftRatings = new List<DraftRating>();

//                    var urlFormatted = string.Format(UrlToScrapeModel.UrlTemplate, set.Value.UrlPartSet, c.Value);
//                    HtmlWeb hw = new HtmlWeb();
//                    HtmlDocument doc = hw.Load(urlFormatted);

//                    var postContentSelector = "//div[contains(concat(' ', normalize-space(@class), ' '), ' entry-content ')]";

//                    var elemsCards = doc.DocumentNode.SelectNodes($"{postContentSelector}/p")
//                        .Where(i => i.ChildNodes.Count == 1)
//                        .Where(i => i.ChildNodes[0].NodeType == HtmlNodeType.Element)
//                        .Where(i => i.ChildNodes[0].Name == "a" || i.ChildNodes[0].Name == "span" || i.ChildNodes[0].Name == "strong")
//                        .Where(i => i.NextSibling.NextSibling.InnerText.StartsWith("Limited:") || i.NextSibling.NextSibling.InnerText.StartsWith("Fun Level"));

//                    var elemsCardName = elemsCards.ToArray();
//                    foreach (var elemCardName in elemsCardName)
//                    {
//                        //try
//                        //{
//                        var cardName = WebUtility.HtmlDecode(elemCardName.InnerText).Replace("’", "'").Trim()
//                            // TYPOS ON THE WEBSITE TO MAINTAIN
//                            //GRN
//                            .Replace("DEAFENING CLARION", "Deafening Clarion")
//                            // RNA
//                            .Replace("Sagittar's Volley", "Sagittars' Volley")
//                            // WAR
//                            .Replace("Sarkhan, the Masterless", "Sarkhan the Masterless")
//                            // M20
//                            .Replace("Gruesome Scourge", "Gruesome Scourger")
//                            //ELD
//                            .Replace("Ardvenale", "Ardenvale")
//                            .Replace("Lochthwain Paladin", "Locthwain Paladin")
//                            .Replace("Bake Into a Pie", "Bake into a Pie")
//                            .Replace("Edgewall Inkeeper", "Edgewall Innkeeper")
//                            .Replace("Torbran, Thane of the Red Fell", "Torbran, Thane of Red Fell")
//                            .Replace("Traxos, Scourge of the Kroog", "Traxos, Scourge of Kroog")
//                            // THB
//                            .Replace("Aphemia the Cacophony", "Aphemia, the Cacophony")
//                            .Replace("Mire  Triton", "Mire Triton")
//                            .Replace("Eutropia, the Twice-Favored", "Eutropia the Twice-Favored")
//                            .Replace("Ardvenale", "Ardenvale")
//                            .Replace("Ardvenale", "Ardenvale")
//                            .Replace("Ardvenale", "Ardenvale");

//                        // Skip those elements that don't correspond to actual cards
//                        if (allCardsNames.Contains(cardName) == false)
//                        {
//                            if (cardName.Contains("LSV") == false && cardName != "Ratings Scale" && cardName != "Artifacts" && cardName != "Gates/Shocks" && cardName != "Lockets" && cardName.StartsWith("Top ") == false &&
//                                 cardName.StartsWith("Lifegain Lands ") == false && cardName != "Temples" && cardName.EndsWith("Reviews") == false && cardName.EndsWith("Rankings") == false && cardName.StartsWith("Most Important") == false &&
//                                 cardName != "Dimir Locket, Golgari Locket, Izzet Locket, Selesnya Locket" && cardName != "Boros Guildgate, Izzet Guildgate, Golgari Guildgate, Dimir Guildgate, Selesnya Guildgate" &&
//                                 cardName != "Boros" && cardName != "Azorius" && cardName != "Dimir" && cardName != "Gruul" && cardName != "Selesnya" && cardName != "Simic" && cardName != "Orzhov" && cardName != "Rakdos" && cardName != "Golgari" && cardName != "Izzet" &&
//                                  cardName != "Clifftop Retreat, Hinterland Harbor, Isolated Chapel, Sulfur Falls, Woodland Cemetery" && cardName != "Memorial to Folly, Genius, Glory, & Unity")
//                            {
//                                System.Diagnostics.Debugger.Break();
//                            }
//                            continue;
//                        }

//                        var nextSibling = elemCardName.NextSibling.NextSibling;

//                        string rating = "";
//                        string desc = "";
//                        var lookingForDesc = true;

//                        while (rating == "" || lookingForDesc)
//                        {
//                            if (nextSibling.InnerText.StartsWith("Limited:"))
//                                rating = WebUtility.HtmlDecode(nextSibling.InnerText).Replace("Limited:", "").Trim();
//                            else if (nextSibling.Name == "p" && nextSibling.InnerText.StartsWith("Fun Level") == false)
//                                desc += $"{WebUtility.HtmlDecode(nextSibling.InnerText).Trim()}{Environment.NewLine}{Environment.NewLine}";

//                            nextSibling = nextSibling.NextSibling.NextSibling;
//                            //try
//                            //{
//                            lookingForDesc = nextSibling.Name == "#text" || nextSibling.Name == "p" || nextSibling.Name == "ul" || nextSibling.Name == "h3";
//                            lookingForDesc &= nextSibling.NextSibling.NextSibling != null && nextSibling.InnerText.StartsWith("Tags:") == false;

//                            //}
//                            //catch (Exception ex)
//                            //{
//                            //    System.Diagnostics.Debugger.Break();
//                            //}
//                        }

//                        // Special treatment
//                        if (cardName == "Roving Keep")
//                        {
//                            rating = "1.5";
//                        }

//                        draftRatings.Add(new DraftRating
//                        {
//                            CardName = cardName,
//                            RatingToDisplay = rating,
//                            RatingValue = float.Parse(rating),
//                            Description = desc.Trim(),
//                        });
//                        //}
//                        //catch (Exception ex)
//                        //{
//                        //    System.Diagnostics.Debugger.Break();
//                        //}
//                    }

//                    setInfo.Ratings = setInfo.Ratings.Union(draftRatings).ToArray();

//                    if (c.Key.Length == 1)
//                    {
//                        var texts = doc.DocumentNode.SelectNodes($"{postContentSelector}/p | {postContentSelector}/h4 | {postContentSelector}/h3")
//                            .Select(i => i.InnerText.Trim())
//                            .SelectMany(i => i.Split('\n').Select(x => x.Trim()))
//                            .ToArray();
//                        var top5 = texts
//                            .Where(i => i.StartsWith("1. ") || i.StartsWith("2. ") || i.StartsWith("3. ") || i.StartsWith("4. ") || i.StartsWith("5. "))
//                            .Select(i => new DraftRatingTopCard(Convert.ToInt32(i.Substring(0, 1)), WebUtility.HtmlDecode(i.Substring(2, i.Length - 2)).Trim()))
//                            .ToArray();

//                        setInfo.TopCommonCardsByColor[c.Key] = top5;
//                    }
//                }

//                ret.RatingsBySet.Add(set.Key, setInfo);
//            }

//            // Added manually because Missing
//            foreach (var set in ret.RatingsBySet)
//            {
//                set.Value.Ratings = set.Value.Ratings.Union(manualData.manualRatings[set.Key]).ToArray();
//            }

//            return ret;
//        }
//    }
//}