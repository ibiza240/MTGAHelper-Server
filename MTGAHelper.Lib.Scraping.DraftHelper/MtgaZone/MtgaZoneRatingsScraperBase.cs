using CsvHelper;
using CsvHelper.Configuration;
using MTGAHelper.Entity;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Scraping.DraftHelper.MtgaZone.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MTGAHelper.Lib.Scraping.DraftHelper.MtgaZone
{
    public abstract class MtgaZoneRatingsScraperBase
    {
        protected readonly Dictionary<string, string> typos = new Dictionary<string, string>
        {
            ["Light up the Night"] = "Light Up the Night",
            ["Invoke The Winds"] = "Invoke the Winds",
            ["Cabaretti Ascendency"] = "Cabaretti Ascendancy",
            ["Mephit’'s Enthusiasm"] = "Mephit's Enthusiasm",
            ["Sentinel Wyrm"] = "Miirym, Sentinel Wyrm",
            ["Life Trapping"] = "Mirror of Life Trapping",
            ["You Find the Villain's Lair"] = "You Find the Villains' Lair",
            ["Sarevok, the Usurper"] = "Sarevok the Usurper",
            ["Giant fire Beetles"] = "Giant Fire Beetles",
            ["You find Some Prisoners"] = "You Find Some Prisoners",
            ["Miirym, sentinel Wyrm"] = "Miirym, Sentinel Wyrm",
            ["Mirror of life Trapping"] = "Mirror of Life Trapping",
            ["Juniper order Rootweaver"] = "Juniper Order Rootweaver",
        };

        //protected Dictionary<string, float> ratingLetters = new Dictionary<string, float>
        //{
        //    { "S", 5.4f},
        //    { "A+", 5.3f},
        //    { "A",  5.0f},
        //    { "A-", 4.6f},
        //    { "B+", 4.4f},
        //    { "B",  4.0f},
        //    { "B-", 3.6f},
        //    { "C+", 3.4f},
        //    { "C",  3.0f},
        //    { "C-", 2.6f},
        //    { "D+", 2.4f},
        //    { "D",  2.0f},
        //    { "D-", 1.6f},
        //    { "F+", 1.4f},
        //    { "F",  1.0f},
        //    { "F-", 0.6f},
        //    { "I", 0.01f},
        //};
        protected readonly Dictionary<string, float> ratingLetters = new Dictionary<string, float>
        {
            { "5",  5.0f},
            { "4.5", 4.5f},
            { "4",  4.0f},
            { "3.5", 3.5f},
            { "3",  3.0f},
            { "2.5", 2.5f},
            { "2",  2.0f},
            { "1.5", 1.5f},
            { "1",  1.0f},
            { "0.5", 0.5f},
            { "0", 0.0f},
        };

        protected readonly string folderData;
        protected readonly ICardRepository allCards;
        private readonly SharedTools sharedTools;

        public class MtgaZoneRatingsScraperBaseArgs
        {
            public string FolderData { get; set; }
            public ICardRepository AllCards { get; set; }
        }

        protected MtgaZoneRatingsScraperBase(MtgaZoneRatingsScraperBaseArgs args)
        {
            this.folderData = args.FolderData;
            this.allCards = args.AllCards;
            this.sharedTools = new SharedTools(allCards);
        }

        protected (CsvConfiguration csvConfig, string filepath) GetCsvConfig(string set)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = "\t",
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            var file = Path.Combine(folderData, $"mtgazoneratings_{set}.txt");

            return (config, file);
        }

        public DraftRatingScraperResultForSet GetRatingsForSet(string set)
        {
            (var config, var file) = GetCsvConfig(set);

            ICollection<MtgaZoneRatingsScraperModel> records = null;
            using (var reader = new CsvReader(new StreamReader(file), config))
            {
                records = reader.GetRecords<MtgaZoneRatingsScraperModel>().ToArray();
            }

            var ratings = records.Select(MapRecord).ToArray();

            var ret = new DraftRatingScraperResultForSet
            {
                Ratings = ratings,
                TopCommonCardsByColor = sharedTools.GetTop5CommonByColor(ratings)
            };

            return ret;
        }

        protected DraftRating MapRecord(MtgaZoneRatingsScraperModel i)
        {
            if (string.IsNullOrEmpty(i.RatingCompulsion))
                i.RatingCompulsion = i.RatingDrifter;

            var avg = (ratingLetters[i.RatingDrifter] + ratingLetters[i.RatingCompulsion]) / 2f;
            var closest = ratingLetters
                .Select(x => new { letter = x.Key, diff = Math.Abs(x.Value - avg), value = x.Value })
                .OrderBy(x => x.diff)
                .First();

            return new DraftRating
            {
                CardName = (typos.ContainsKey(i.CardName) ? typos[i.CardName] : i.CardName.Replace("’", "'")).Trim(),
                RatingValue = closest.value,
                RatingToDisplay = $"{closest.letter}",
                Description = $"Drifter: {i.RatingDrifter}\r\nCompulsion: {i.RatingCompulsion}",
            };
        }
    }
}