using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib
{
    public class PastDraftRetriever
    {
        private readonly IQueryHandler<CompletedDraftsAfterQuery, IEnumerable<(DateTime date, DraftPickStatusRaw picks)>> qDraftPicksAfter;
        private readonly IMapper mapper;

        public PastDraftRetriever(IQueryHandler<CompletedDraftsAfterQuery, IEnumerable<(DateTime, DraftPickStatusRaw)>> qDraftPicksAfter, IMapper mapper)
        {
            this.qDraftPicksAfter = qDraftPicksAfter;
            this.mapper = mapper;
        }

        public async Task<ICollection<DraftCardsPicked>> GetCardsPickedByDraft(string userId, DateTime dateFrom, string set)
        {
            var completedDrafts = await qDraftPicksAfter.Handle(new CompletedDraftsAfterQuery(userId, dateFrom));

            return completedDrafts
                .Select(x => new DraftCardsPicked
                {
                    Set = set,
                    DatePicked = x.date,
                    CardsPicked = mapper.Map<ICollection<Card>>(x.picks.PickedCards.Select(i => Convert.ToInt32(i)))
                })
                .ToList();
        }
    }
}