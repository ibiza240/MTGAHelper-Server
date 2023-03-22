using System.Collections.Generic;
using System.Linq;

namespace MtgaDecksPro.Cards.Entity
{
    public record FormatInfo
    {
        /// <summary>
        /// Must correspond to one of the const Format values
        /// </summary>
        public string Name { get; init; } = Constants.Unknown;

        public string Description { get; init; } = "";
        public int BestOf { get; init; }
        public bool IsActive { get; init; }

        public CardPoolEnum CardPoolType { get; init; }
        public IEnumerable<string> CardPool { get; init; } = Enumerable.Empty<string>();

        public ICollection<CardBanned> CardsBanned { get; init; } = new CardBanned[0];
    }
}