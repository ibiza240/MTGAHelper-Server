using MTGAHelper.Entity.OutputLogParsing;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestMatchUpdatesQuery : IQuery<IReadOnlyDictionary<DateTime, PostMatchUpdateRaw>>
    {
        public LatestMatchUpdatesQuery(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
}
