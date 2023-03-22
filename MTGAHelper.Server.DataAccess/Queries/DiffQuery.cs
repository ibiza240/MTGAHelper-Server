using MTGAHelper.Entity.UserHistory;
using System;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class DiffQuery : IQuery<DateSnapshotDiff>
    {
        public string UserId { get; }
        public DateTime Date { get; }

        public DiffQuery(string userId, DateTime date)
        {
            UserId = userId;
            Date = date;
        }
    }
}
