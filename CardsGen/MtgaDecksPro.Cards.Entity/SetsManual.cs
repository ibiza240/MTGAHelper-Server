using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Entity
{
    public record SetsManual
    {
        public ICollection<string> SetsStandard { get; set; } = new string[0];
        public ICollection<string> SetsHistoric { get; set; } = new string[0];
        public ICollection<SetManualInfo> SetsInfo { get; set; } = new SetManualInfo[0];
    }

    public record SetManualInfo
    {
        public string Set { get; set; }
        public int CardsPerSet { get; set; }
        public string PromoCardNumber { get; set; }
        public bool PromoCardIsStyle { get; set; }
    }
}