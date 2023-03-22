using MTGAHelper.Entity;
using MTGAHelper.Lib.CollectionDecksCompare;
using MTGAHelper.Lib.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTGAHelper.Lib.IO.Writer.WriterDeckTypes
{
    public interface IWriterDeckMtga
    {
        IWriterDeckMtga Init(
            IReadOnlyCollection<CardWithAmount> collection,
            IReadOnlyCollection<int> landsPreference,
            bool landsPickAll);

        void Write(IDeck deck, string directory, bool alertIfExists = false);

        string ToText(IDeck deck);
    }

    public class WriterDeckMtga : IWriterDeckMtga
    {
        private readonly CardToCollectionMatcher cardToCollectionMatcher;
        private readonly Util util;

        public WriterDeckMtga(
            CardToCollectionMatcher cardToCollectionMatcher,
            Util util)
        {
            this.cardToCollectionMatcher = cardToCollectionMatcher;
            this.util = util;
        }

        public IWriterDeckMtga Init(
            IReadOnlyCollection<CardWithAmount> collection,
            IReadOnlyCollection<int> landsPreference,
            bool landsPickAll)
        {
            cardToCollectionMatcher.Init(collection, landsPreference, landsPickAll);
            return this;
        }

        public void Write(IDeck deck, string directory, bool alertIfExists = false)
        {
            Directory.CreateDirectory(directory);
            var f = util.RemoveInvalidCharacters(deck.Name);
            var filename = Path.Combine(directory, $"{f}.txt");

            //if (deck.Name.StartsWith("Naya Z"))
            //    System.Diagnostics.Debugger.Break();

            if (alertIfExists && File.Exists(filename))
                //Log.Error();
                throw new DeckFileAlreadyExistsException($"File {filename} already exists");

            File.WriteAllText(filename, ToText(deck));
        }

        public string ToText(IDeck deck)
        {
            return string.Join("\n", GetTextCards(deck));
        }

        private ICollection<string> GetTextCards(IDeck deck)
        {
            var textCards = new List<string>();

            var commander = deck.Cards.QuickCardCommander;
            if (commander != null)
            {
                textCards.Add("Commander");
                textCards.AddRange(BuildLinesForCards(new[] {deck.Cards.QuickCardCommander}));
                textCards.Add("");
            }

            var companion = deck.Cards.QuickCardCompanion;
            if (companion != null)
            {
                textCards.Add("Companion");
                textCards.AddRange(BuildLinesForCards(new[] {deck.Cards.QuickCardCompanion}));
                textCards.Add("");
            }

            textCards.Add("Deck");
            textCards.AddRange(BuildLinesForCards(deck.Cards.QuickCardsMain.Values));

            if (deck.Cards.QuickCardsSideboard.Any())
            {
                textCards.Add("");
                textCards.Add("Sideboard");
                textCards.AddRange(BuildLinesForCards(deck.Cards.QuickCardsSideboard.Values));
            }

            return textCards;
        }

        private ICollection<string> BuildLinesForCards(IEnumerable<DeckCard> cards)
        {
            var collectionCards = cardToCollectionMatcher.FindCardsInCollection(cards);
            return collectionCards.Select(BuildCardString).ToArray();
        }

        private string BuildCardString(CardWithAmount c)
        {
            var set = c.Card.Set;
            if (set == "DOM")
                set = "DAR";
            else if (set == "MIR")
                set = "MI";

            var name = c.Card.Name;

            if (name.EndsWith(" (a)") || name.EndsWith(" (b)"))
                name = name.Substring(0, name.Length - 4);
            else if (name == "Lurrus of the Dream-Den")
                name = "Lurrus of the Dream Den";

            //if (name.StartsWith("Legion"))
            //{
            //    System.Diagnostics.Debugger.Break();
            //}

            // e.g. Search for Azcanta 74a
            var number = c.Card.Number.Replace("a", "");

            var fullInfo = c.Card.Type.StartsWith("Basic Land") || c.Card.Set != "J21";

            if (fullInfo)
                return $"{c.Amount} {name} ({set}) {number}";
            else
                return $"{c.Amount} {name}";
        }
    }
}