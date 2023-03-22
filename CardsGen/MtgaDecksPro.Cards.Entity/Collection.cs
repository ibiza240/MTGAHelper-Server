using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MtgaDecksPro.Cards.Entity
{
    public record Collection
    {
        public DateTime LastUpdatedUtc { get; init; }
        public ICollection<CollectionItem> Cards { get; init; } = new CollectionItem[0];

        [JsonIgnore]
        public string LastUpdatedUtcFormatted => $"{LastUpdatedUtc:yyyy-MM-dd hh:mm:ss}";

        public Collection()
        {
        }

        public Collection(ICollection<CollectionItem> cards)
        {
            Cards = cards;
            LastUpdatedUtc = DateTime.UtcNow;
        }
    }
}