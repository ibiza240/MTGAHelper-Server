//using System;
//using System.Collections.Generic;
//using System.Text;
//using MTGAHelper.Lib.IO.Reader.MtgaOutputLog;
//using MTGAHelper.Lib.MtgaDeckStats;
//using MTGAHelper.Lib.Cache;

//namespace MTGAHelper.Lib.Matches
//{
//    public class MatchesCacheManager
//    {
//        CacheSingleton<IReadOnlyDictionary<string, ICollection<MatchResult>>> cacheMatches;
//        MatchesReader reader;

//        public MatchesCacheManager(CacheSingleton<IReadOnlyDictionary<string, ICollection<MatchResult>>> cacheMatches, MatchesReader reader)
//        {
//            this.cacheMatches = cacheMatches;
//            this.reader = reader;
//        }

//        public ICollection<MatchResult> GetMatches(string userId)
//        {
//            var dictMatchesByUserId = cacheMatches.Get();
//            if (dictMatchesByUserId.ContainsKey(userId) == false || dictMatchesByUserId[userId].Count == 0)
//            {
//                var matchesForUser = reader.ReadMatches(userId);
//                dictMatchesByUserId[userId] = matchesForUser;
//                cacheMatches.Set(dictMatchesByUserId);
//            }

//            return dictMatchesByUserId[userId];
//        }

//        internal void Invalidate(string userId)
//        {
//            var dictMatchesByUserId = cacheMatches.Get();

//            if (dictMatchesByUserId.ContainsKey(userId))
//                dictMatchesByUserId.Remove(userId);

//            cacheMatches.Set(dictMatchesByUserId);
//        }
//    }
//}