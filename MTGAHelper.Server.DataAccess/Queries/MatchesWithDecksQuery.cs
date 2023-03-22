using MTGAHelper.Entity.MtgaOutputLog;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class MatchesWithDecksQuery : IQuery<IReadOnlyList<MatchResult>>
    {
        public string UserId { get; }
        public IReadOnlyCollection<string> Decks { get; }
        public DateTime DateStart { get; }

        public MatchesWithDecksQuery(string userId, IReadOnlyCollection<string> withDecks, DateTime dateStart)
        {
            UserId = userId;
            Decks = withDecks;
            DateStart = dateStart;
        }
    }
}