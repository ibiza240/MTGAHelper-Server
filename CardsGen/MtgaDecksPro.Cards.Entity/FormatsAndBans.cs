using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Entity
{
    public record FormatsAndBans
    {
        public Dictionary<string, FormatInfo> FormatInfo { get; set; }
        public Dictionary<string, CardBanned[]> BansByCard { get; set; }

        public Dictionary<AdditionalCardsEnum, ICollection<string>> HistoricAdditionalCards { get; set; }
    }
}