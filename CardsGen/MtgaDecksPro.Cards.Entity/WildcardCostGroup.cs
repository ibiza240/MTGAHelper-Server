using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Entity
{
    public record WildcardCostGroup
    {
        public string GroupName { get; set; }
        public Dictionary<RarityEnum, int> WildcardsCost { get; set; }
    }
}