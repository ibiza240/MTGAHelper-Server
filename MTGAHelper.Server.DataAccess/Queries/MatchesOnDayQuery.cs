using MTGAHelper.Entity.MtgaOutputLog;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class MatchesOnDayQuery : IQuery<IReadOnlyList<MatchResult>>
    {
        public string UserId { get; }
        public DateTime Date { get; }

        public MatchesOnDayQuery(string userId, DateTime date)
        {
            UserId = userId;
            Date = date;
        }
    }
}
