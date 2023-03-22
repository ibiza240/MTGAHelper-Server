using CsvHelper;
using CsvHelper.Configuration;
using MTGAHelper.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MTGAHelper.Lib.Scraping.DraftHelper.InfiniteMythicEdition
{
    public class InfiniteMythicEditionRatingsScraper
    {
        private readonly Dictionary<string, float> ratingLetters = new Dictionary<string, float>
        {
            { "S", 5.4f},
            { "A+", 5.3f},
            { "A",  5.0f},
            { "A-", 4.6f},
            { "B+", 4.4f},
            { "B",  4.0f},
            { "B-", 3.6f},
            { "C+", 3.4f},
            { "C",  3.0f},
            { "C-", 2.6f},
            { "D+", 2.4f},
            { "D",  2.0f},
            { "D-", 1.6f},
            { "F+", 1.4f},
            { "F",  1.0f},
            { "F-", 0.6f},
            { "I", 0.01f},
            { "", 0f},
        };

        private readonly string[] sets = { "ONE", "BRO", "DMU", "HBG", "SNC", "NEO", "IKO", "THB", "WAR", "M21", "AKR", "ZNR", "KLR", "KHM", "STX", "STA", "AFR", "MID", "VOW" };
        private readonly SharedTools sharedTools;
        private readonly string folderData;

        private readonly Dictionary<string, string> typos = new Dictionary<string, string>
        {
            { "Bonder's Enclave", "Bonders' Enclave" },
            { "Twinblade Assassin", "Twinblade Assassins" },
            { "Emeria Captian", "Emeria Captain" },
            { "User of the Fallen", "Usher of the Fallen" },
            { "Saw it Coming", "Saw It Coming" },
            { "Alund's Epiphany", "Alrund's Epiphany" },
            { "Binding of the Old Gods", "Binding the Old Gods" },
            { "Glimpse of the Cosmos", "Glimpse the Cosmos" },
            { "Igna Rune-Eyes", "Inga Rune-Eyes" },
            { "Bloodsky Berseker", "Bloodsky Berserker" },
            { "Varragoth, Blodsky Sire", "Varragoth, Bloodsky Sire" },
            { "Dwarwen Reinforcements", "Dwarven Reinforcements" },
            { "Deathknell Berseker", "Deathknell Berserker" },
            { "Terfrid's Shadow", "Tergrid's Shadow" },
            { "Mage Hunter's Onslaught", "Mage Hunters' Onslaught" },
            { "Plumb the Forgotten", "Plumb the Forbidden" },
            { "Spike Pit Trap", "Spiked Pit Trap" },
            { "Djinni Windseeker", "Djinni Windseer" },
            { "Boosts of Speed", "Boots of Speed" },
            { "Undying Malace", "Undying Malice" },
            { "Siba Trespassers", "Saiba Trespassers" },
            { "Stinghive Master", "Stinging Hivemaster" },
            { "Veil of Assassination", "Veil of Assimilation" },
        };

        public InfiniteMythicEditionRatingsScraper(string folderData, SharedTools sharedTools)
        {
            this.folderData = folderData;
            this.sharedTools = sharedTools;
        }

        public DraftRatings Scrape(string setFilter = "")
        {
            var ret = new DraftRatings();

            foreach (var set in sets.Where(i => setFilter == "" || i == setFilter))
            {
                var ratings = GetRatingsForSet(set);
                ret.RatingsBySet.Add(set, ratings);
            }

            return ret;
        }

        public DraftRatingScraperResultForSet GetRatingsForSet(string set)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = "\t",
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            var file = Path.Combine(folderData, $"imeratings_{set}.txt");

            ICollection<InfiniteMythicEditionRatingsModel> records = null;
            using (var reader = new CsvReader(new StreamReader(file), config))
            {
                records = reader.GetRecords<InfiniteMythicEditionRatingsModel>().ToArray();
            }

            var ratings = records.Select(MapRecord).ToArray();

            var ret = new DraftRatingScraperResultForSet
            {
                Ratings = ratings,
                TopCommonCardsByColor = sharedTools.GetTop5CommonByColor(ratings)
            };

            return ret;
        }

        private DraftRating MapRecord(InfiniteMythicEditionRatingsModel i)
        {
            var nbRatings = i.Rating3 == "" ? 2f : 3f;
            var avg = (ratingLetters[i.Rating1] + ratingLetters[i.Rating2] + ratingLetters[i.Rating3]) / nbRatings;
            var closest = ratingLetters
                .Select(x => new { letter = x.Key, diff = Math.Abs(x.Value - avg), value = x.Value })
                .OrderBy(x => x.diff)
                .First();

            var inColor = string.IsNullOrWhiteSpace(i.InColor) ? string.Empty : $"In-color: {i.InColor}\r\n";
            var sideboard = string.IsNullOrWhiteSpace(i.Sideboard) ? string.Empty : "Good for Sideboard\r\n";
            var split = inColor != string.Empty || sideboard != string.Empty ? "\r\n" : string.Empty;

            return new DraftRating
            {
                CardName = (typos.ContainsKey(i.CardName) ? typos[i.CardName] : i.CardName).Trim(),
                RatingValue = closest.value,
                RatingToDisplay = $"{closest.letter}",
                Description = $"{inColor}{sideboard}{split}Justlolaman: {i.Rating1}\r\nM0bieus: {i.Rating2}\r\nScottynada: {i.Rating3}",
            };
        }
    }
}