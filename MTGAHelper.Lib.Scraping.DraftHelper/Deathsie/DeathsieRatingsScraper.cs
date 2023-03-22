using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTGAHelper.Entity;

namespace MTGAHelper.Lib.Scraping.DraftHelper.Deathsie
{
    public class DeathsieRatingsScraper
    {
        private readonly Dictionary<int, string> ratingLetters = new Dictionary<int, string>
        {
            { 5, "S" },
            { 4, "A" },
            { 3, "B" },
            { 2, "C" },
            { 1, "D" },
            { 0, "F" },
        };

        private readonly string[] sets = { "ONE", "BRO", "DMU", "HBG", "SNC", "NEO", "VOW", "MID", "AFR", "STA", "STX", "KHM", "KLR", "ZNR", "AKR", "WAR", "GRN", "DOM", "ELD", "THB", "IKO", "M21" };
        private readonly SharedTools sharedTools;
        private readonly string folderData;

        private readonly Dictionary<string, string> typos = new Dictionary<string, string>
        {
            { "Glaive the Guildpact", "Glaive of the Guildpact" },
            { "Collar the Cultprit", "Collar the Culprit" },
            { "Outlaw's Merriment", "Outlaws' Merriment" },
            { "Lovestruck beast", "Lovestruck Beast" },
            { "Torban, Thane of Red Fell", "Torbran, Thane of Red Fell" },
            { "Turn Into a Pumpkin", "Turn into a Pumpkin" },
            { "Syr Elenora, The Discerning", "Syr Elenora, the Discerning" },
            { "All that Glitters", "All That Glitters" },
            { "Overwhelemed Apprentice", "Overwhelmed Apprentice" },
            { "Return of Nature", "Return to Nature" },
            { "Illuna, Apex of Whishes", "Illuna, Apex of Wishes" },
            { "Yoiron, Sky Nomad", "Yorion, Sky Nomad" },
            { "Bonder's Enclave", "Bonders' Enclave" },
            { "Twinblade Assassin", "Twinblade Assassins" },
            { "Rambunctious Mut", "Rambunctious Mutt" },
            { "Emeria Captian", "Emeria Captain" },
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
            { "Plumb the Forgotten", "Plumb the Forbidden" },
            { "Spike Pit Trap", "Spiked Pit Trap" },
            { "Djinni Windseeker", "Djinni Windseer" },
            { "Boosts of Speed", "Boots of Speed" },
            { "Undying Malace", "Undying Malice" },
            { "Siba Trespassers", "Saiba Trespassers" },
            { "Rasaad, Monk of Selune", "Rasaad, Monk of Selûne" },
            { "Stinghive Master", "Stinging Hivemaster" },
            { "Veil of Assassination", "Veil of Assimilation" },
        };

        public DeathsieRatingsScraper(string folderData, SharedTools sharedTools)
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
            var data = File.ReadAllText(Path.Combine(folderData, $"deathsieratings_{set}.txt"));
            ICollection<DraftRating> ratings = default;

            if (data.Contains("\r\n\t\t\r\n"))
            {
                // Old format
                var dataSplitByRating = data.Split(new[] { "\r\n\t\t\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                var dataByRating = dataSplitByRating
                    .Select((i, index) => new { rating = dataSplitByRating.Length - index - 1, data = i })
                    .GroupBy(i => i.rating, i => i.data.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    .ToArray();

                ratings = dataByRating
                   .SelectMany(i => i.SelectMany(x => x.Select(c => new { rating = i.Key, name = c.Split('\t')[2].Replace("’", "'").Trim() })))
                   .Select(c => new DraftRating
                   {
                       CardName = (typos.ContainsKey(c.name) ? typos[c.name] : c.name)/*.Replace("/", " // ")*/,
                       Description = "",
                       RatingValue = c.rating,
                       RatingToDisplay = $"{ratingLetters[c.rating]}",
                   })
                   .ToArray();
            }
            else
            {
                ratings = data
                    .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Split('\t'))
                    .Select(i => new
                    {
                        rating = i[0],
                        name = i[2],
                    })
                    .Select(c => new DraftRating
                    {
                        CardName = (typos.ContainsKey(c.name) ? typos[c.name] : c.name).Trim()/*.Replace("/", " // ")*/,
                        Description = "",
                        RatingValue = ratingLetters.First(i => i.Value == c.rating).Key,
                        RatingToDisplay = c.rating,
                    })
                    .ToArray();
            }

            if (ratings == default) throw new Exception("Wrong");

            var ret = new DraftRatingScraperResultForSet();
            ret.Ratings = ratings;
            ret.TopCommonCardsByColor = sharedTools.GetTop5CommonByColor(ratings);
            return ret;
        }
    }
}