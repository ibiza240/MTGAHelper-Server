namespace Minmaxdev.Common
{
    public static class ObjectExtensions
    {
        public static string GetCacheKey(this object o) => o.GetType().FullName;
    }
}