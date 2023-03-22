using System.Collections.Generic;
using MTGAHelper.Entity;
using System.Linq;

namespace MTGAHelper.Web.Models.Response.Misc
{
    public class GetCardsResponse
    {
        public ICollection<Card> Cards { get; }

        public GetCardsResponse(IEnumerable<Card> cards)
        {
            Cards = cards.ToArray();
        }
    }
}
