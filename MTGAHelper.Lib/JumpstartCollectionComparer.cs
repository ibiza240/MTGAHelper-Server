using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Scraping;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MTGAHelper.Lib
{
    public class JumpstartCollectionComparisonTheme
    {
        public string ThemeName { get; set; }
        public ICollection<JumpstartCollectionComparison> Details { get; set; }
        public float Priority { get; set; }
        public CardWithAmountSimple MissingLand { get; set; }
    }

    public class JumpstartCollectionComparison
    {
        public string ThemeName { get; set; }
        public int ThemeVariant { get; set; }
        public int ThemeVariantProbabilityPct { get; set; }
        public ICollection<CardWithAmountSimple> MissingCards { get; set; }
    }

    public class JumpstartThemeInfo
    {
        public int Probability { get; set; }
        public int LandGrpId { get; set; }

        public JumpstartThemeInfo(int probability, int landGrpId)
        {
            Probability = probability;
            LandGrpId = landGrpId;
        }
    }

    public enum JumpstartLandWeightingEnum
    {
        Ignore,
        SameAsRare,
        MoreThanRare,
    }

    public class JumpstartCollectionComparer
    {
        private readonly string folderData;
        private readonly ICardRepository cardRepo;

        private readonly Dictionary<string, JumpstartThemeInfo> dictThemeVariantProbability = new()
        {
            { "Basri", new JumpstartThemeInfo(100, 74641) },
            { "Unicorns", new JumpstartThemeInfo(100, 72561) },
            { "Teferi", new JumpstartThemeInfo(100, 74642) },
            { "Milling", new JumpstartThemeInfo(100, 72570) },
            { "Liliana", new JumpstartThemeInfo(100, 74643) },
            { "Phyrexian", new JumpstartThemeInfo(100, 72578) },
            { "Chandra", new JumpstartThemeInfo(100, 74644) },
            { "Seismic", new JumpstartThemeInfo(100, 72584) },
            { "Garruk", new JumpstartThemeInfo(100, 74645) },
            { "Walls", new JumpstartThemeInfo(100, 72595) },
            { "Rainbow", new JumpstartThemeInfo(100, 72598) },
            { "Angels", new JumpstartThemeInfo(50, 72560) },
            { "Dogs", new JumpstartThemeInfo(50, 72565) },
            { "Enchanted", new JumpstartThemeInfo(50, 72562) },
            { "Pirates", new JumpstartThemeInfo(50, 72572) },
            { "Spirits", new JumpstartThemeInfo(50, 72571) },
            { "Under the Sea", new JumpstartThemeInfo(50, 72566) },
            { "Discarding", new JumpstartThemeInfo(50, 72575) },
            { "Rogues", new JumpstartThemeInfo(50, 72577) },
            { "Witchcraft", new JumpstartThemeInfo(50, 72579) },
            { "Dragons", new JumpstartThemeInfo(50, 72582) },
            { "Lightning", new JumpstartThemeInfo(50, 72588) },
            { "Minotaurs", new JumpstartThemeInfo(50, 72589) },
            { "Cats", new JumpstartThemeInfo(50, 72594) },
            { "Elves", new JumpstartThemeInfo(50, 72597) },
            { "Lands", new JumpstartThemeInfo(50, 72591) },
            { "Doctor", new JumpstartThemeInfo(25, 72559) },
            { "Feathered Friends", new JumpstartThemeInfo(25, 72564) },
            { "Heavily Armored", new JumpstartThemeInfo(25, 72563) },
            { "Legion", new JumpstartThemeInfo(25, 72558) },
            { "Above the Clouds", new JumpstartThemeInfo(25, 72568) },
            { "Archaeology", new JumpstartThemeInfo(25, 72569) },
            { "Well-Read", new JumpstartThemeInfo(25, 72573) },
            { "Wizards", new JumpstartThemeInfo(25, 72567) },
            { "Minions", new JumpstartThemeInfo(25, 72574) },
            { "Reanimated", new JumpstartThemeInfo(25, 72576) },
            { "Spooky", new JumpstartThemeInfo(25, 72581) },
            { "Vampires", new JumpstartThemeInfo(25, 72580) },
            { "Devilish", new JumpstartThemeInfo(25, 72583) },
            { "Goblins", new JumpstartThemeInfo(25, 72585) },
            { "Smashing", new JumpstartThemeInfo(25, 72587) },
            { "Spellcasting", new JumpstartThemeInfo(25, 72586) },
            { "Dinosaurs", new JumpstartThemeInfo(25, 72593) },
            { "Plus One", new JumpstartThemeInfo(25, 72592) },
            { "Predatory", new JumpstartThemeInfo(25, 72596) },
            { "Tree-Hugging", new JumpstartThemeInfo(25, 72590) },
        };

        private readonly Regex regexThemeNameAndVariant = new Regex(@"^(.*?)(?:$| \((\d)\)$)", RegexOptions.Compiled);

        private HashSet<string> standardSets = new HashSet<string>
        {
            "GRN",
            "RNA",
            "WAR",
            "M20",
            "ELD",
            "THB",
            "IKO",
            "M21",
        };

        private Dictionary<JumpstartLandWeightingEnum, int> dictLandWeighting = new Dictionary<JumpstartLandWeightingEnum, int>
        {
            { JumpstartLandWeightingEnum.Ignore, 0 },
            { JumpstartLandWeightingEnum.SameAsRare, 100 },
            { JumpstartLandWeightingEnum.MoreThanRare, 1000 },
        };

        public JumpstartCollectionComparer(
            IDataPath dataPath,
            ICardRepository cardRepo)
        {
            this.folderData = dataPath.FolderData;
            this.cardRepo = cardRepo;
        }

        public ICollection<JumpstartCollectionComparisonTheme> CalculateMissingCardsForThemes(IReadOnlyDictionary<int, int> collection, bool onlyStandard, JumpstartLandWeightingEnum landWeighting)
        {
            var fileContent = File.ReadAllText(Path.Combine(folderData, "jumpstartThemes.json"));
            var themes = JsonConvert.DeserializeObject<ICollection<JumpstartPack>>(fileContent)!;

            var result = new List<JumpstartCollectionComparison>();

            foreach (var theme in themes)
            {
                var match = regexThemeNameAndVariant.Match(theme.ThemeName);
                var themeName = match.Groups[1].Value;
                var variant = match.Groups[2].Value == "" ? 1 : int.Parse(match.Groups[2].Value);

                var rares = theme.Cards
                    .Select(c => cardRepo.CardsByName(c.Name))
                    .Where(c => onlyStandard ? c.Any(x => standardSets.Contains(x.Set)) : c.Any())
                    .Where(c => c.All(x => x.Rarity is RarityEnum.Rare or RarityEnum.Mythic));

                var owned = rares.Select(c => new CardWithAmountSimple(
                    c.Select(card => collection.GetValueOrDefault(card.GrpId)).Sum(),
                    c.First().Name,
                    c.First(x => onlyStandard == false || standardSets.Contains(x.Set)).ImageCardUrl));

                result.Add(new JumpstartCollectionComparison
                {
                    ThemeName = themeName,
                    MissingCards = owned.Select(i => new CardWithAmountSimple(Math.Max(0, 4 - i.Amount), i.Name, i.ImageCardUrl)).ToArray(),
                    ThemeVariant = variant,
                    ThemeVariantProbabilityPct = dictThemeVariantProbability[themeName].Probability,
                });
            }

            return result
                .GroupBy(i => i.ThemeName)
                .Select(i =>
                {
                    var land = cardRepo[dictThemeVariantProbability[i.Key].LandGrpId];
                    var landMissing = collection.ContainsKey(land.GrpId) ? 0 : 1;
                    var landPriority = landMissing * dictLandWeighting[landWeighting];

                    return new JumpstartCollectionComparisonTheme
                    {
                        ThemeName = i.Key,
                        Priority = i.Sum(x => i.First().ThemeVariantProbabilityPct * x.MissingCards.Sum(y => y.Amount)) + landPriority,
                        Details = i.ToArray(),
                        MissingLand = new CardWithAmountSimple(landMissing, $"{i.Key} {land.Name}", land.ImageCardUrl),
                    };
                })
                .OrderByDescending(i => i.Priority)
                .ToArray();
        }
    }
}