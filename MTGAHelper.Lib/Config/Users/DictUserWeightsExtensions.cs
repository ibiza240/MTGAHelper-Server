using MTGAHelper.Entity;
using MTGAHelper.Entity.CollectionDecksCompare;
using System.Collections.Generic;

namespace MTGAHelper.Lib.Config.Users
{
    public static class DictUserWeightsExtensions
    {
        public static IReadOnlyDictionary<RarityEnum, UserWeightDto> OrDefaultValues(this IReadOnlyDictionary<RarityEnum, UserWeightDto> dict)
        {
            return dict ?? CardRequiredInfo.DEFAULT_WEIGHTS;
        }

        public static bool IsDefaultValues(this IReadOnlyDictionary<RarityEnum, UserWeightDto> dict)
        {
            if (dict == null)
                return true;

            var same = true;
            var q = new Queue<RarityEnum>(dict.Keys);
            while (same && q.Count > 0)
            {
                var k = q.Dequeue();
                same &= dict[k].Main == CardRequiredInfo.DEFAULT_WEIGHTS[k].Main;
                same &= dict[k].Sideboard == CardRequiredInfo.DEFAULT_WEIGHTS[k].Sideboard;
            }

            return same;
        }
    }
}