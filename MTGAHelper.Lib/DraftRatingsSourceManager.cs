using System;
using System.Collections.Generic;
using System.Text;
using MTGAHelper.Entity;

namespace MTGAHelper.Lib
{
    public class DraftRatingsSourceManager
    {
        IReadOnlyDictionary<string, DraftRatings> draftRatingsBySource;

        public DraftRatingsSourceManager(
            CacheSingleton<IReadOnlyDictionary<string, DraftRatings>> cacheDraftRatings)
        {
            draftRatingsBySource = cacheDraftRatings.Get();
        }

        public DraftRatings GetRatingForSource(string source)
        {
            if (source == null || draftRatingsBySource.ContainsKey(source) == false)
                return new DraftRatings();

            return draftRatingsBySource[source];
        }

        public IReadOnlyDictionary<string, DraftRatings> GetRatingsAll()
        {
            return draftRatingsBySource;
        }
    }
}
