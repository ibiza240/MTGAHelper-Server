using System.Collections.Generic;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity
{
    public class CacheManualDataModel
    {
        public Dictionary<string, string> SetsGatherer { get; set; }
        public Dictionary<string, CardManualData> Cards { get; set; }
    }
}