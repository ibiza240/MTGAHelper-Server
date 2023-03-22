using MTGAHelper.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTGAHelper.Entity.OutputLogParsing;

namespace MTGAHelper.Lib
{
    public class RankDeltaCalculator
    {


        public RankDeltaCalculator()
        {
        }

        public RankDelta GetDelta(DateTime dateTime, RankFormatEnum format, Rank start, Rank end, GetSeasonAndRankDetailRaw seasonConfig)
        {
            if (format == RankFormatEnum.Unknown)
                return new RankDelta(start, end);

            var config = format == RankFormatEnum.Constructed ? seasonConfig.constructedRankInfo : seasonConfig.limitedRankInfo;
            if (config == null)
            {
                System.Diagnostics.Debugger.Break();
            }

            var idxLeft = config.FindIndex((i) => i.rankClass == start.Class && i.level == start.Level);
            var idxRight = config.FindIndex((i) => i.rankClass == end.Class && i.level == end.Level);
            var multiplicator = 1;

            if (idxLeft > idxRight)
            {
                // When the delta is negative (ranks lost)
                var idxTemp = idxLeft;
                idxLeft = idxRight;
                idxRight = idxTemp;
                multiplicator = -1;
            }

            var levelsToConsider = config.Skip(idxLeft).Take(idxRight - idxLeft + 1).ToArray();

            var totalSteps = levelsToConsider.Sum(i => i.steps);
            var steps = multiplicator * (totalSteps - start.Step - (levelsToConsider.Last().steps - end.Step));
            if (multiplicator == -1)
                steps = multiplicator * (totalSteps - end.Step - (levelsToConsider.First().steps - start.Step));

            return new RankDelta(start, end)
            {
                DateTime = dateTime,
                DeltaSteps = steps
            };
        }
    }
}
