using System.Collections.Generic;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Mtga
{
    public struct Key
    {
        public int id { get; set; }
        public string text { get; set; }
        public string raw { get; set; }
    }

    public record MtgaDataLocRootObject
    {
        public string langkey { get; set; }
        public string isoCode { get; set; }
        public List<Key> keys { get; set; }
    }
}