//using Microsoft.Extensions.Logging;
//using MTGAHelper.Entity;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;

//namespace MTGAHelper.Lib.TextDeck
//{
//    public interface IMtgoTextDeckConverter
//    {
//        IMtgoTextDeckConverter Init(ICollection<Card> allCards);
//        ICollection<DeckCard> Convert(string deckName, string textDeck);
//    }

//    public class MtgoTextDeckConverter : BaseDeckConverter, IMtgoTextDeckConverter
//    {
//        public new IMtgoTextDeckConverter Init(ICollection<Card> allCards)
//        {
//            base.Init(allCards);
//            return this;
//        }

//        public ICollection<DeckCard> Convert(string deckName, string textDeck)
//        {
//            var deckCards = new List<DeckCard>();

//            var isSideboard = false;
//            var regex = new Regex(@"^(\d+) (.*?)$", RegexOptions.Compiled);

//            foreach (var line in textDeck.Split('\n'))
//            {
//                if (line.Trim() == "Sideboard")
//                {
//                    isSideboard = true;
//                    //continue;
//                }
//                else
//                {
//                    var m = regex.Match(line);
//                    var name = m.Groups[2].Value;

//                    if (name.IndexOf("/") > 0)
//                    {
//                        // For cards with aftermath
//                        name = name.Substring(0, name.IndexOf("/"));
//                    }

//                    try
//                    {
//                        // can crash, apparently
//                        var amount = int.Parse(m.Groups[1].Value);

//                        var card = allCards.First(i => i.CompareNameWith(name));
//                        deckCards.Add(new DeckCard(new CardWithAmount(card, amount), isSideboard));
//                    }
//                    catch (Exception ex)
//                    {
//                        //var path = Path.Combine(logFolder, $"logCards_{deckName}.txt");
//                        //var txt = $"{line} [{name}] {ex.Message}" + Environment.NewLine + Environment.NewLine + textDeck;
//                        //File.WriteAllText(path, txt);

//                        // Invalid deck
//                        // LOG
//                        return null;
//                    }
//                }
//            }

//            return deckCards.ToArray();
//        }
//    }
//}