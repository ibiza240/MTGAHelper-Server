using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib
{
    public enum BannedCardFormat
    {
        Standard,
        Historic,
    }

    public static class BannedCardsProviderTemp
    {
        public static ICollection<string> GetBannedCards(BannedCardFormat format)
        {
            if (format == BannedCardFormat.Standard)
                return new[]
                {
                  "Alrund's Epiphany",
                  "Divide by Zero",
                  "Faceless Haven",
                  "Omnath, Locus of Creation",
                };
            else if (format == BannedCardFormat.Historic)
                return new[]
                {
                  "Agent of Treachery",
                  "Brainstorm",
                  "Field of the Dead",
                  "Memory Lapse",
                  "Nexus of Fate",
                  "Oko, Thief of Crowns",
                  "Omnath, Locus of Creation",
                  "Once Upon a Time",
                  "Thassa's Oracle",
                  "Tibalt's Trickery",
                  "Time Warp",
                  "Uro, Titan of Nature's Wrath",
                  "Veil of Summer",
                  "Wilderness Reclamation",
                  "Winota, Joiner of Forces",
                };

            return Array.Empty<string>();
        }
    }
}