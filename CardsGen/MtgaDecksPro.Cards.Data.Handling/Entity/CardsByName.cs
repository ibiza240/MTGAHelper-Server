using MtgaDecksPro.Cards.Entity;
using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Data.Handling.Entity
{
    public class CardsByName : Dictionary<string, List<Card>>
    {
        public CardsByName()
        {
        }

        public CardsByName(Dictionary<string, List<Card>> data) : base(data)
        {
        }
    }
}