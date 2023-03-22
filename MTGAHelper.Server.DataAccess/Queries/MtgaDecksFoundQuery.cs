using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class MtgaDecksFoundQuery : IQuery<HashSet<string>>
    {
        public MtgaDecksFoundQuery(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
}
