using MTGAHelper.Entity.OutputLogParsing;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class PostMatchUpdatesAfterQuery : IQuery<IReadOnlyCollection<KeyValuePair<DateTime, PostMatchUpdateRaw>>>
    {
        public PostMatchUpdatesAfterQuery(string userId, DateTime fromDateTime)
        {
            UserId = userId;
            FromDateTime = fromDateTime;
        }

        public string UserId { get; }
        public DateTime FromDateTime { get; }
    }
}
