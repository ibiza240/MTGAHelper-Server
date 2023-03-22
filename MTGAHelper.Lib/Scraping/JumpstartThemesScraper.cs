using HtmlAgilityPack;
using MTGAHelper.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MTGAHelper.Lib.Scraping
{
    public class JumpstartPack
    {
        public string ThemeName { get; set; }
        public ICollection<CardWithAmountSimple> Cards { get; set; }
    }

    public class JumpstartThemesScraper
    {
        public readonly Dictionary<string, string> ArenaReplacements = new Dictionary<string, string>
        {
            { "Chain Lightning", "Lightning Strike" },
            { "Lightning Bolt", "Lightning Strike" },
            { "Ball Lightning", "Lightning Serpent" },
            { "Ajani's Chosen", "Archon of Sun's Grace" },
            { "Angelic Arbiter", "Serra's Guardian" },
            { "Draconic Roar", "Scorching Dragonfire" },
            { "Goblin Lore", "Goblin Oriflamme" },
            { "Flametongue Kavu", "Fanatic of Mogis" },
            { "Exhume", "Bond of Revival" },
            { "Fa'adiyah Seer", "Dryad Greenseeker" },
            { "Mausoleum Turnkey", "Audacious Thief" },
            { "Path to Exile", "Banishing Light" },
            { "Read the Runes", "Gadwick, the Wizened" },
            { "Reanimate", "Doomed Necromancer" },
            { "Rhystic Study", "Teferi's Ageless Insight" },
            { "Sheoldred, Whispering One", "Carnifex Demon" },
            { "Scourge of Nel Toth", "Woe Strider" },
            { "Scrounging Bandar", "Pollenbright Druid" },
            { "Thought Scour", "Weight of Memory" },
            { "Time to Feed", "Prey Upon" },
        };

        readonly Regex regexCardAmount = new Regex(@"^(\d+)\s+(.*?)$", RegexOptions.Compiled);
        readonly Regex regexIgnoreSpecialLand = new Regex(@"^1\s+.+? (?:Plains|Island|Swamp|Mountain|Forest)$", RegexOptions.Compiled);

        public ICollection<JumpstartPack> GetPacks()
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load("https://magic.wizards.com/en/articles/archive/feature/jumpstart-decklists-2020-06-18");

            var result = new List<JumpstartPack>();

            var themes = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' bean_block_deck_list ')]");
            foreach (var theme in themes)
            {
                var themeName = theme.SelectSingleNode(".//h4").InnerText;

                // Oversights on the website, different format
                themeName = themeName
                    .Replace("Discarding 1", "Discarding (1)")
                    .Replace("Discarding 2", "Discarding (2)")
                    .Replace("Rogue (", "Rogues (")
                    .Replace("Spellingcasting", "Spellcasting")
                    .Replace("Plus one", "Plus One");

                var cardsFound = theme.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' sorted-by-overview-container ')]//span[@class='row']");
                var cards = cardsFound
                    .Select(i =>
                    {
                        var innerText = i.InnerText.Trim();
                        var match = regexCardAmount.Match(innerText);
                        if (match.Success == false || regexIgnoreSpecialLand.Match(innerText).Success || innerText.Contains("Terramorphic Expanse"))
                            return new CardWithAmountSimple(0, "", "");

                        var cardName = match.Groups[2].Value;
                        if (ArenaReplacements.ContainsKey(cardName))
                            cardName = ArenaReplacements[cardName];

                        return new CardWithAmountSimple(int.Parse(match.Groups[1].Value), cardName, "");
                    })
                    .Where(i => i.Amount != 0)
                    .ToArray();

                var pack = new JumpstartPack
                {
                    ThemeName = themeName,
                    Cards = cards,
                };
                result.Add(pack);
            }

            return result;
        }
    }
}
