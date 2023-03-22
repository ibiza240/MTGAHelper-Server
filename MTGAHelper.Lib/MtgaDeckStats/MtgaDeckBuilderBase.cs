using MTGAHelper.Entity;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Lib.CardProviders;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MTGAHelper.Lib.MtgaDeckStats
{
    public class MTGADeckBuilderBase
    {
        protected readonly IReadOnlyDictionary<int, Card> dictAllCards;

        public MTGADeckBuilderBase(ICardRepository cardRepo)
        {
            dictAllCards = cardRepo;
        }

        protected (string deckId,
                   string deckName,
                   string deckImage,
                   ConfigModelRawDeck mtgaDeck) GetDeck(IEnumerable<MatchResult> matches, ConfigModelRawDeck deck)
        {
            var deckTileId = deck?.DeckTileId;
            if (deckTileId == 0)
                deckTileId = null;

            ConfigModelRawDeck mtgaDeck = deck;
            var lastMatch = matches.MaxBy(i => i.StartDateTime);

            if (mtgaDeck?.Cards?.Any() != true)
            {
                if (lastMatch != null)
                {
                    mtgaDeck = lastMatch.DeckUsed;

                    if (deckTileId == null)
                        deckTileId = mtgaDeck.DeckTileId;
                }
            }

            if (mtgaDeck == null)
            {
                System.Diagnostics.Debugger.Break();
                return ("N/A", "N/A", "", null);
            }

            var deckImage = deckTileId.HasValue && dictAllCards.TryGetValue(deckTileId.Value, out Card card)
                ? card.ImageArtUrl
                : Card.Unknown.ImageCardUrl;

            var deckName = lastMatch.DeckUsed?.Name ?? "N/A";
            // Special treatment for Precon decks from a list (eg. an event)
            if (deckName.Contains("Loc/Decks/Precon"))
                deckName = Regex.Replace(deckName.Replace("Loc/Decks/Precon", ""), @"\?|=|\/", "");

            return (mtgaDeck.Id, deckName, deckImage, mtgaDeck);
        }
    }
}