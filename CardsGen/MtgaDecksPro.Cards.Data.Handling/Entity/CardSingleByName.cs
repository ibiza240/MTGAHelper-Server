using MtgaDecksPro.Cards.Entity;
using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Data.Handling.Entity
{
    //public class CachedHandlerCardsByName : Cached<CardsByName, List<Card>, CachedHandlerCardsByName>
    //{
    //    public CachedHandlerCardsByName(CachedHandlerCardsByName handler) : base(handler) { }
    //}

    public class CardSingleByName : Dictionary<string, Card>
    {
        public CardSingleByName()
        {
        }

        public CardSingleByName(IDictionary<string, Card> dict) : base(dict)
        {
        }
    }
}