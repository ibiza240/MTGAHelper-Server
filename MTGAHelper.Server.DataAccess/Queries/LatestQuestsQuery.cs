using MTGAHelper.Entity;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestQuestsQuery : IQuery<InfoByDate<IReadOnlyList<PlayerQuest>>>
    {
        public LatestQuestsQuery(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
}
