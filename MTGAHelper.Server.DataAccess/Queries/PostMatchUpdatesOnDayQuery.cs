using MTGAHelper.Entity.OutputLogParsing;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class PostMatchUpdatesOnDayQuery : IQuery<IReadOnlyDictionary<DateTime, PostMatchUpdateRaw>>
    {
        public PostMatchUpdatesOnDayQuery(string userId, DateTime date)
        {
            UserId = userId;
            Date = date;
        }

        public string UserId { get; }
        public DateTime Date { get; }
    }
}
