using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.Scraping.DraftHelper.ChannelFireball
{
    public static class DictExtensions
    {
        public static Dictionary<TKey, TValue> And<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            var newDict = new Dictionary<TKey, TValue>();
            foreach (var i in dict)
                newDict[i.Key] = i.Value;

            newDict[key] = value;
            return newDict;
        }

        public static Dictionary<TKey, TValue> And<TKey, TValue>(this Dictionary<TKey, TValue> dict, Dictionary<TKey, TValue> toAppend)
        {
            var newDict = new Dictionary<TKey, TValue>();
            foreach (var i in dict)
                newDict[i.Key] = i.Value;

            foreach (var a in toAppend)
                newDict[a.Key] = a.Value;

            return newDict;
        }
    }
}
