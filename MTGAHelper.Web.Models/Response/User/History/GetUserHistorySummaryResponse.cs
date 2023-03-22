﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using MTGAHelper.Entity.UserHistory;
using System.Text.RegularExpressions;
using MTGAHelper.Web.Models.SharedDto;

namespace MTGAHelper.Web.Models.Response.User.History
{
    public class GetUserHistorySummaryResponse
    {
        private readonly Regex regex_Rank_StringParts = new Regex(@"^(.*?)_(.*?)_(.*?)$", RegexOptions.Compiled);

        //public ICollection<GetUserHistorySummaryDto> History { get; set; }
        public ICollection<GetUserHistorySummaryDto> History2 { get; set; }

        public int TotalItems { get; set; }

        private readonly DateTime dateNewHistory = new DateTime(2019, 11, 18);

        //public GetUserHistoryResponse(ICollection<DateSnapshot> details)
        //{
        //    History = details
        //        .Select(i => Mapper.Map<GetUserHistoryDto>(i.Info).Map(i.Diff))
        //        .ToArray();
        //}

        public GetUserHistorySummaryResponse()
        {
        }

        public GetUserHistorySummaryResponse(IEnumerable<GetUserHistorySummaryDto> summary, IEnumerable<GetUserHistorySummaryDto> summary2, int totalItems, ICollection<string> datesAvailable)
        {
            TotalItems = totalItems;

            History2 = Merge(
                summary.Where(i => datesAvailable.Contains(i.Date.ToString("yyyyMMdd"))).ToArray(),
                summary2.Where(i => datesAvailable.Contains(i.Date.ToString("yyyyMMdd")))
            );
        }

        private ICollection<GetUserHistorySummaryDto> Merge(ICollection<GetUserHistorySummaryDto> history, IEnumerable<GetUserHistorySummaryDto> history2)
        {
            foreach (var i in history)
            {
                if (i.ConstructedRank != "N/A")
                {
                    var m = regex_Rank_StringParts.Match(i.ConstructedRank);
                    i.ConstructedRankChange = new RankDeltaDto
                    {
                        deltaSteps = 420,
                        RankEnd = new RankDto
                        {
                            Format = "Constructed",
                            Class = m.Groups[1].Value.ToString(),
                            Level = Convert.ToInt32(m.Groups[2].Value),
                            Step = Convert.ToInt32(m.Groups[3].Value),
                        }
                    };
                }

                if (i.LimitedRank != "N/A")
                {
                    var m = regex_Rank_StringParts.Match(i.LimitedRank);
                    i.LimitedRankChange = new RankDeltaDto
                    {
                        deltaSteps = 420,
                        RankEnd = new RankDto
                        {
                            Format = "Limited",
                            Class = m.Groups[1].Value.ToString(),
                            Level = Convert.ToInt32(m.Groups[2].Value),
                            Step = Convert.ToInt32(m.Groups[3].Value),
                        }
                    };
                }
            }

            return history.Where(i => i.Date.Date < dateNewHistory).Union(history2.Where(i => i.Date.Date >= dateNewHistory)).OrderByDescending(i => i.Date).ToArray();
        }
    }
}