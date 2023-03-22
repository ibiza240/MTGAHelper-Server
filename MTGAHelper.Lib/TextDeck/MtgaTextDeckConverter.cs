using MTGAHelper.Entity;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MTGAHelper.Lib.Exceptions;

namespace MTGAHelper.Lib.TextDeck
{
    public interface IMtgaTextDeckConverter
    {
        ICollection<DeckCard> Convert(string deckName, string textDeck);

        ICollection<DeckCard> ConvertFromFile(string deckName, string directory);
    }

    public class MtgaTextDeckConverter : IMtgaTextDeckConverter
    {
        private static readonly Regex HAS_SLASH_IN_MIDDLE = new(@"^.+\/.+$", RegexOptions.Compiled);

        private readonly Util util;
        private readonly ICardRepository cardRepo;

        public MtgaTextDeckConverter(
            Util util,
            ICardRepository cardRepo)
        {
            this.util = util;
            this.cardRepo = cardRepo;
        }

        public ICollection<DeckCard> Convert(string deckName, string textDeck)
        {
            var deckCards = new List<DeckCard>();

            var zone = DeckCardZoneEnum.Deck;
            var regex = new Regex(@"^(\d+) (.*?(?: \(.\))?)( \((.*?)\)( ([0-9a-zA-Z]+).*)?)?$", RegexOptions.Compiled);

            try
            {
                foreach (var l in textDeck.Split(new[] { "\n" }, StringSplitOptions.None))
                {
                    var line = l.Trim();
                    switch (line)
                    {
                        case "Companion":
                            zone = DeckCardZoneEnum.Companion;
                            continue;
                        case "Commander":
                            zone = DeckCardZoneEnum.Commander;
                            continue;
                        case "Deck":
                            zone = DeckCardZoneEnum.Deck;
                            continue;
                        case "Sideboard":
                            zone = DeckCardZoneEnum.Sideboard;
                            continue;
                        case "":
                            if (zone == DeckCardZoneEnum.Deck) zone = DeckCardZoneEnum.Sideboard;
                            continue;
                    }

                    Match m = regex.Match(line);
                    int amount = int.Parse(m.Groups[1].Value);
                    string name = m.Groups[2].Value;
                    string set = m.Groups.Count > 4 ? m.Groups[4].Value.ToUpper() : "";
                    string number = m.Groups.Count > 6 ? m.Groups[6].Value : "";
                    Card card = CreateCard(name, set, number);
                    if (card is null)
                    {
                        // Deck cannot be imported because it contains card [xyz] outside the valid sets
                        //var ex = new CardMissingException("Card not found: {invalidCard}");
                        //ex.Data.Add("invalidCard", name);
                        var ex = new CardMissingException($"Card not found: \"{name}\". Invalid line: " + line);
                        throw ex;
                    }
                    deckCards.Add(new DeckCard(card, amount, zone));
                }
            }
            catch (CardMissingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidDeckFormatException($"Cannot convert deck [{deckName}] from text", ex);
                //System.Diagnostics.Debugger.Break();
            }

            deckCards = deckCards
                //.OrderBy(i => i.Card.cmc)
                .GroupBy(i => new { i.Zone, grpId = i.Card.GrpId })
                .Select(i => new DeckCard(i.First().Card, i.Sum(x => x.Amount), i.Key.Zone))
                .ToList();

            return deckCards.ToArray();
        }

        public ICollection<DeckCard> ConvertFromFile(string deckName, string directory)
        {
            var filename = util.RemoveInvalidCharacters(deckName);
            var filePath = Path.Combine(directory, $"{filename}.txt");
            LogExt.LogReadFile(filePath);
            var fileContent = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(fileContent))
            {
                File.Delete(filePath);
                throw new EmptyFileException("File on disk for deck {deckName} is empty");
            }

            return Convert(deckName, fileContent);
        }

        private Card CreateCard(string name, string set, string number)
        {
            if (name == "Lurrus of the Dream Den")
                name = "Lurrus of the Dream-Den";
            else if (name.Contains("  "))
                name = name.Replace("  ", " // "); // MtgGoldfish?
            else if (HAS_SLASH_IN_MIDDLE.IsMatch(name) && name.Contains("//") == false)
                name = name.Replace("/", " // "); // StreamDecker

            name = name.Replace("///", "//");

            if (set == "DAR")
                set = "DOM";
            else if (set == "MI")
                set = "MIR";

            IReadOnlyCollection<Card> cardsWithName = cardRepo.CardsByName(name);
            return cardsWithName.Count switch
            {
                0 => BestMatch(FindByNamePartLogged()),
                1 => cardsWithName.First(),
                _ => BestMatch(cardsWithName)
            };

            IReadOnlyCollection<Card> FindByNamePartLogged()
            {
                var found = FindByNamePart();
                Log.Information("{MethodName}: {Count} results found for '{Name}'",
                    nameof(FindByNamePart),
                    found.Count,
                    name);
                return found;
            }
            
            IReadOnlyCollection<Card> FindByNamePart()
            {
                // For Modal cards, we might want to look only for the first part
                if (name.Contains(" // "))
                {
                    var firstPartName = name.Split(new[] {" // "}, StringSplitOptions.None)[0]; 
                    return cardRepo.CardsByName(firstPartName);
                }
                // On the other side, sometimes the split card is only identified by the first name
                return cardRepo.FindNameStartingWith($"{name} //"/* && i.LinkedFaceType == enumLinkedFace.SplitCard*/);
            }

            Card BestMatch(IReadOnlyCollection<Card> candidates)
            {
                if (set == "")
                    return candidates.FirstOrDefault();
                
                return candidates.FirstOrDefault(c => c.Set == set && c.Number == number)
                    ?? candidates.FirstOrDefault(c => c.Set == set)
                    ?? candidates.FirstOrDefault(c => c.Number == number)
                    ?? candidates.FirstOrDefault();
            }
        }
        
    }
}
