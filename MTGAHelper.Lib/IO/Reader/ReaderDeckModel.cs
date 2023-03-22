using CsvHelper;
using MTGAHelper.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTGAHelper.Lib.Exceptions;
using CsvHelper.Configuration;
using MTGAHelper.Lib.CardProviders;
using System.Diagnostics;

namespace MTGAHelper.Lib.IO.Reader
{
    public class ReaderDeckModel
    {
        readonly ICardRepository cardRepo;

        readonly CsvConfiguration config;

        readonly Dictionary<string, Card> cardsCodeMapping;

        public readonly Dictionary<string, float> loadingTimes = new();

        public ReaderDeckModel(ICardRepository cardRepo, CsvConfiguration config)
        {
            this.cardRepo = cardRepo;
            this.config = config;
        }

        public ReaderDeckModel(ICardRepository cardRepo, CsvConfiguration config, Dictionary<string, Card> cardsCodeMapping)
        {
            this.cardRepo = cardRepo;
            this.config = config;
            this.cardsCodeMapping = cardsCodeMapping;
        }

        public IDeck Read(string deckName, ScraperType scraperType, string filePathDeck)
        {
            var watchFull = System.Diagnostics.Stopwatch.StartNew();
            IDeck res = null;
            try
            {
                using (var reader = new CsvReader(new StreamReader(filePathDeck), config))
                {
                    var records = reader.GetRecords<DeckCardRawModel>().ToArray();
                    if (records.Any(i => i.IsMainOther))
                    {
                        throw new Exception("This is obsolete");
                        //var watchSplitCards = System.Diagnostics.Stopwatch.StartNew();
                        //var cardsMain = records
                        //    .Where(i => i.IsSideboard == false && i.IsMainOther == false)
                        //    .Select(i => new DeckCard(new CardWithAmount(GetCard(i.Code), i.Qty), i.IsSideboard ? DeckCardZoneEnum.Sideboard : DeckCardZoneEnum.Deck));

                        //var cardsMainOther = records
                        //    .Where(i => i.IsSideboard == false && i.IsMainOther == true)
                        //    .Select(i => GetCard(i.Code));

                        //var cardsSideboard = records
                        //    .Where(i => i.IsSideboard == true && i.IsMainOther == false)
                        //    .Select(i => GetCard(i.Code));

                        //res = new DeckAverageArchetype(deckName, scraperType, cardsMain, cardsMainOther, cardsSideboard);
                        //watchSplitCards.Stop();
                        //loadingTimes["SplitCards"] = watchSplitCards.ElapsedMilliseconds / 1000.0f;
                    }
                    else
                    {
                        var deckCards = records
                            .Select(i => new DeckCard(GetCard(i.Code), i.Qty, i.IsSideboard ? DeckCardZoneEnum.Sideboard : DeckCardZoneEnum.Deck))
                            .ToArray();
                        res = new Deck(deckName, scraperType, deckCards);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDeckFormatException($"Cannot convert deck [{deckName}] from text", ex);
            }

            watchFull.Stop();
            var timeSplitCards = loadingTimes.ContainsKey("SplitCards") ? loadingTimes["SplitCards"] : 0.0f;
            loadingTimes["AllExceptSplitCards"] = watchFull.ElapsedMilliseconds / 1000.0f - timeSplitCards;

            return res;
        }

        private Card GetCard(string s)
        {
            if (cardsCodeMapping == null)
            {
                return cardRepo.CardsByName(s).Last();
            }
            else
            {
                var card = cardRepo[cardsCodeMapping[s].GrpId];
                // Validate that we handle cards from different sets (same card) correctly
                Debug.Assert(card == cardsCodeMapping[s]);
                return card;
            }
        }
    }
}
