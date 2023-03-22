using MtgaDecksPro.Cards.Entity;
using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Data.Handling.Entity
{
    public class CardsById : Dictionary<string, Card>
    {
        public CardsById()
        {
        }

        public CardsById(IDictionary<string, Card> dict) : base(dict)
        {
        }
    }
}