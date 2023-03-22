namespace MTGAHelper.Lib
{
    public interface IClearUserCache
    {
        void ClearCacheForUser(string userId);
        void FreeMemory();
    }
}
