using MTGAHelper.Entity.MtgaOutputLog;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class MatchesWithDeckQuery : IQuery<IEnumerable<MatchResult>>
    {
        public MatchesWithDeckQuery(string userId, string deckId, DateTime dateStart)
        {
            UserId = userId;
            DeckId = deckId;
            DateStart = dateStart;
        }

        public string UserId { get; }
        public string DeckId { get; }
        public DateTime DateStart { get; }
    }
}
