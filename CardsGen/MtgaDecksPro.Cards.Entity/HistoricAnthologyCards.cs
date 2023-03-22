using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Entity
{
    public record HistoricAnthologyCards //: IData
    {
        public int Id { get; set; }
        public ICollection<string> Cards { get; set; }
    }
}