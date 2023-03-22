namespace MTGAHelper.Server.DataAccess.CacheUserHistory
{
    public class UserHistoryKeyInput
    {
        public string UserId { get; }
        public string DateFor { get; }

        public UserHistoryKeyInput(string userId, string dateFor)
        {
            this.UserId = userId;
            this.DateFor = dateFor;
        }

        public string ToKey() => UserId + DateFor;
    }
}