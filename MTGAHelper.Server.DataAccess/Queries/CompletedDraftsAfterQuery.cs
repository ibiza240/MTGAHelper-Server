using MTGAHelper.Entity.OutputLogParsing;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class CompletedDraftsAfterQuery : IQuery<IEnumerable<(DateTime, DraftPickStatusRaw)>>
    {
        public CompletedDraftsAfterQuery(string userId, DateTime fromDateTime)
        {
            UserId = userId;
            FromDateTime = fromDateTime;
        }

        public string UserId { get; }
        public DateTime FromDateTime { get; }
    }
}
