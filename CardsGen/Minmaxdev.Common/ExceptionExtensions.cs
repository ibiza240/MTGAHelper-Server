using System;

namespace Minmaxdev.Common
{
    public static class ExceptionExtensions
    {
        public static T WithData<T>(this T ex, string key, object value) where T : Exception
        {
            ex.Data.Add(key, value);
            return ex;
        }
    }
}