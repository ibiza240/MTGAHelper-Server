using MTGAHelper.Entity;
using MTGAHelper.Entity.MtgaDeckStats;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.MasteryPass;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.MtgaDeckStats
{
    public class MtgaDeckSummaryBuilder : MTGADeckBuilderBase
    {
        //MatchesCacheManager matchesCacheManager;
        private readonly IQueryHandler<MatchesWithDecksQuery, IReadOnlyList<MatchResult>> qMatches;

        private readonly IQueryHandler<MtgaDecksFoundQuery, HashSet<string>> qMtgaDecks;
        private readonly UserMtgaDeckRepository userDeckRepository;
        private readonly Util util;
        private readonly UtilColors utilColors;

        public MtgaDeckSummaryBuilder(
            ICardRepository cardRepo,
            //MatchesCacheManager matchesCacheManager,
            IQueryHandler<MatchesWithDecksQuery, IReadOnlyList<MatchResult>> qMatches,
            IQueryHandler<MtgaDecksFoundQuery, HashSet<string>> qMtgaDecks,
            UserMtgaDeckRepository userDeckRepository,
            Util util,
            UtilColors utilColors)
            : base(cardRepo)
        {
            //this.matchesCacheManager = matchesCacheManager;
            this.qMatches = qMatches;
            this.qMtgaDecks = qMtgaDecks;
            this.userDeckRepository = userDeckRepository;
            this.util = util;
            this.utilColors = utilColors;
        }

        public async Task<ICollection<MtgaDeckSummary>> GetSummary(string userId, string period)
        {
            var mtgaDecksAll = await userDeckRepository.Get(userId);
            if (mtgaDecksAll == null)
                return Array.Empty<MtgaDeckSummary>();

            var mtgaDecksToShow = await qMtgaDecks.Handle(new MtgaDecksFoundQuery(userId));

            //var minDate = new DateTime(2019, 09, 26);
            var currentSet = "ONE";
            var lastSet = "BRO";
            var minDate = period == "currentset" ? SetStartingDates.DictStartingDate[currentSet] :
                period == "currentandpreviousset" ? SetStartingDates.DictStartingDate[lastSet] : new DateTime(2019, 9, 26);

            var matches = await qMatches.Handle(new MatchesWithDecksQuery(userId, mtgaDecksToShow, minDate));

            var dictDeckIds = matches
                .Select(i => i.DeckUsed)
                .Where(i => string.IsNullOrWhiteSpace(i.Id) == false)
                .GroupBy(i => i.Name)
                .ToDictionary(i => i.Key, i => i.Last().Id);

            var matchesByDeck = matches
                .GroupBy(i => dictDeckIds[i.DeckUsed.Name])
                .Where(i => i.Any())
                .ToDictionary(m => m.Key, m => m);

            var dictMtgaDecks = mtgaDecksAll.ToDictionary(i => i.Id, i => i);

            var ret = mtgaDecksToShow
                .Where(id => matchesByDeck.ContainsKey(id))
                .Select(id => BuildSummary(matchesByDeck[id].ToArray(), dictMtgaDecks.ContainsKey(id) ? dictMtgaDecks[id] : null))
                .ToArray();

            return ret;
        }

        private MtgaDeckSummary BuildSummary(IReadOnlyCollection<MatchResult> matchesAll, ConfigModelRawDeck deck)
        {
            // Do not consider Sparky matches for stats
            var matches = matchesAll.Where(i => i.EventName != "AIBotMatch").ToArray();

            var (deckId, deckName, deckImage, mtgaDeck) = GetDeck(matches, deck);
            //if (deckName == "WW")
            //    System.Diagnostics.Debugger.Break();

            var formatWithMostMatches = "N/A";
            var winRate = 0f;
            var winRateNbWin = 0;
            var winRateNbLoss = 0;
            var winRateNbOther = 0;
            var firstPlayed = default(System.DateTime);
            var lastPlayed = default(System.DateTime);

            var matchesByFormat = matches
                .GroupBy(x => x.EventName ?? "N/A")
                .ToDictionary(i => i.Key, i => i);

            if (matchesByFormat.Count > 0)
            {
                formatWithMostMatches = Enumerable.MaxBy(matchesByFormat, i => i.Value.Count()).Key;

                winRate = (float)matchesByFormat[formatWithMostMatches].Count(i => i.Outcome == GameOutcomeEnum.Victory) / matchesByFormat[formatWithMostMatches].Count();
                winRateNbWin = matchesByFormat[formatWithMostMatches].Count(i => i.Outcome == GameOutcomeEnum.Victory);
                winRateNbLoss = matchesByFormat[formatWithMostMatches].Count(i => i.Outcome == GameOutcomeEnum.Defeat);
                winRateNbOther = matchesByFormat[formatWithMostMatches].Count(i => i.Outcome != GameOutcomeEnum.Victory && i.Outcome != GameOutcomeEnum.Defeat);

                firstPlayed = matches.Min(x => x.StartDateTime);
                lastPlayed = matches.Max(x => x.StartDateTime);
            }

            var grpIds = (mtgaDeck.CardsMainWithCommander ??
                    mtgaDeck.CardsMain?.Select(i => new DeckCardRaw
                    {
                        GrpId = i.Key,
                        Amount = i.Value,
                        Zone = DeckCardZoneEnum.Deck,
                    })
                )
                .Select(i => i.GrpId)
                .ToArray();

            var ret = new MtgaDeckSummary
            {
                DeckId = deckId,
                DeckName = deckName,
                DeckImage = util.GetThumbnailUrl(deckImage),
                //DeckUsed = Mapper.Map<ConfigModelRawDeck>(mtgaDeck),
                DeckColor = utilColors.FromGrpIds(grpIds),
                //StatsByFormat = i.Value.GroupBy(x => x.EventName)
                //    .Select(x => new MtgaDeckStatsByFormat {
                //        Format = x.Key,
                //        NbWins = x.Count(m => m.Outcome == GameOutcomeEnum.Victory),
                //        NbLosses = x.Count(m => m.Outcome == GameOutcomeEnum.Defeat),
                //    })
                //    .ToArray()
                WinRateFormat = formatWithMostMatches,
                WinRate = winRate,
                WinRateNbWin = winRateNbWin,
                WinRateNbLoss = winRateNbLoss,
                WinRateNbOther = winRateNbOther,
                FirstPlayed = firstPlayed,
                LastPlayed = lastPlayed,
            };

            return ret;
        }
    }
}