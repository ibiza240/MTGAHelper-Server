using MTGAHelper.Entity.CollectionDecksCompare;
using MTGAHelper.Entity.UserHistory;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Entity.Config.Users
{
    //public enum DashboardStatusEnum
    //{
    //    Unknown,
    //    Success,
    //    UserNotFound,
    //    CollectionNotFound,
    //}

    public class UserDataInMemoryModel
    {
        //object lockMtgaDeckHistory = new object();
        readonly object lockHistorySummary = new();
        private IList<HistorySummaryForDate> historySummary = new List<HistorySummaryForDate>();

        public DateTime LastCompareUtc { get; set; }

        public Dictionary<string, IDeck> DecksUserDefined { get; set; } = new Dictionary<string, IDeck>();

        public CardsMissingResult CompareResult { get; set; } = new CardsMissingResult();

        //public LockableOutputLogResult HistoryDetails { get; set; } = new LockableOutputLogResult();


        //IList<ConfigModelRawDeck> MtgaDecks { get; set; } = new List<ConfigModelRawDeck>();

        public IList<HistorySummaryForDate> GetHistorySummary()
        {
            lock (lockHistorySummary)
                return historySummary;
        }
        public void SetHistorySummary(IList<HistorySummaryForDate> list)
        {
            lock (lockHistorySummary)
                historySummary = list;
        }
        //public void AddHistorySummary(HistorySummaryForDate summary)
        //{
        //    lock (lockHistorySummary)
        //        HistorySummary.Add(summary);
        //}
        //public IList<ConfigModelRawDeck> GetMtgaDecks()
        //{
        //    lock (lockMtgaDeckHistory)
        //        return MtgaDecks;
        //}
        //public void SetMtgaDecks(IList<ConfigModelRawDeck> list)
        //{
        //    lock (lockMtgaDeckHistory)
        //        MtgaDecks = list;
        //}
    }
}
