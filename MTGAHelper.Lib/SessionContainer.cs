using AutoMapper;
using Microsoft.Extensions.Options;
using MTGAHelper.Entity;
using MTGAHelper.Entity.CollectionDecksCompare;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.CollectionDecksCompare;
using MTGAHelper.Lib.Config.Decks;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Lib.IO.Writer.WriterDeckTypes;
using MTGAHelper.Lib.OutputLogParser.Models;
using MTGAHelper.Lib.TextDeck;
using MTGAHelper.Lib.UserHistory;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using MTGAHelper.Server.DataAccess.Queries;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Lib
{
    public interface ISessionContainer
    {
        // ConfigModelApp ConfigApp { get; }
        // ConfigManagerDecks ConfigDecks { get; }
        //IConfigManagerUsers ConfigUsers { get; }

        //ICardsContainer CardsContainer { get; }
        ICardRepository AllCards { get; }

        Dictionary<(ScraperTypeEnum, ScraperTypeFormatEnum), ICollection<string>> GetScraperUsers();

        Task<Dictionary<string, ConfigModelDeck>> GetDecksIncludingUserCustom(string userId, bool filterWithScrapersActive = true);

        //(DateTime collectionDate, ICollection<CardWithAmount> cards, MtgaUserProfile profile) GetUserCollection(string userId);
        Dictionary<string, DashboardModelSummary> GetUserDashboardSummary(string userId);

        CardMissingDetailsModel[] GetUserDashboardDetails(string userId);

        //SessionContainerUserModel GetUserSession(string userId);

        Task SetUserCollection(string userId, OutputLogResult outputLogResult);

        void SaveToDatabase(string userId, string email, OutputLogResult2 outputLogResult2);

        //void LoadUserValues();
        //CardsContainerInitResultEnum ReloadMasterData(bool loadDecks);

        CardsMissingResult Compare(string userId);

        //ICollection<UserInfo> GetUsers(bool filterActive);
        Task<(int totalDecks, ICollection<DeckTrackedSummary> decks)> GetDecksTracked(string userId, FilterDeckParams filters);

        (int totalDecks, ICollection<DeckSummary> decks) GetSystemDecks(string userId, FilterDeckParams filters);

        DateTime GetVersionUtc();

        Task<DeckTrackedDetails> GetDeck(string userId, string deckId);

        //void AddDecks(ICollection<ConfigModelDeck> decks);
        Task SetDeckPriorityFactor(string userId, string deckId, float value, bool saveConfig);

        Task<string> AddCustomDeck(string userId, string name, string url, string mtgaFormat);

        Task<bool> DeleteCustomDeck(string userId, string deckId, bool saveConfig);

        Task<bool> RegisterUser(string userId, string referer = null);

        Task DeckPriorityFactorResetAll(string userId, float value);

        Task SetDeckPriorityFactorForFilteredDecks(string userId, bool? doTrack, ICollection<string> decksIds);

        Task<DeckTrackedDetails> GetDeckByHash(string userId, uint hash);

        //bool DeleteCustomScraper(string userId, ScraperType scraperType);
        Task ActivateScraper(string userId, string scraperTypeId, bool activate);

        Task UpdateScrapersActive(string userId, ICollection<string> scrapersActive);

        Task SaveUserPreference(string userId, UserPreferenceEnum preferenceKey, string value);

        Dictionary<string, string> GetUserPreferences(string userId);

        Task<ICollection<ScraperDto>> GetScrapersInfo(string userId);

        Task SetUserWeights(string userId, IReadOnlyDictionary<RarityEnum, UserWeightDto> weights);

        IReadOnlyDictionary<RarityEnum, UserWeightDto> GetUserWeights(string userId);

        Task StopNotification(string userId, string notificationId);

        Task ResetNotifications(string userId);

        void ClearUser(string userId);

        UserStatusEnum GetUserStatus(string userId);

        Task UpdateLandsPreference(string userId, ICollection<int> lands);
    }

    public enum UserStatusEnum
    {
        Unknown,
        NonExistent,
        OnDisk,
        InMemory,
    }

    public class SessionContainer : ISessionContainer
    {
        private object lockDecksUserDefined { get; } = new object();

        private readonly IMapper mapper;
        private ConfigModelApp ConfigApp { get; }
        private ConfigManagerDecks ConfigDecks { get; }
        private IConfigManagerUsers ConfigUsers { get; }

        //string folderCollections => Path.Combine($"{ConfigApp.FolderFilesRequired}", "Collections");

        public ICardRepository AllCards { get; }

        private readonly ICardsMissingComparer comparer;

        //IAllCardsLoader loaderCards;
        private readonly IMtgaTextDeckConverter deckConverter;

        private readonly IWriterDeckMtga writerDeckMtga;

        //readonly Util util;
        private readonly UserManager userManager;

        private readonly RawDeckConverter rawDeckConverter;

        //MatchesCacheManager matchesCacheManager;
        private readonly ActiveUserCounter activeUserCounter;

        private readonly CacheUserHistoryOld<HashSet<string>> cacheUserHistoryMtgaDecksFound;

        //CacheUserHistory<Dictionary<int, int>> cacheUserHistoryCollectionIntraday;
        private readonly UserMtgaDeckRepository cacheUserAllMtgaDecks;

        private readonly UserCollectionFetcher userCollectionFetcher;
        private readonly IQueryHandler<MtgaDecksFoundQuery, HashSet<string>> qDecksFound;

        public SessionContainer(
            IMapper mapper,
            ConfigModelApp configApp,
            ConfigManagerDecks configDecks,
            IConfigManagerUsers configUsers,
            //IAllCardsLoader loaderCards,
            ICardsMissingComparer comparer,
            IMtgaTextDeckConverter deckConverter,
            RawDeckConverter rawDeckConverter,
            IWriterDeckMtga writerDeckMtga,
            ICardRepository cardRepo,
            UserManager userManager,
            //MatchesCacheManager matchesCacheManager,
            ActiveUserCounter activeUserCounter,
            CacheUserHistoryOld<HashSet<string>> cacheUserHistoryMtgaDecksFound,
            //CacheUserHistory<Dictionary<int, int>> cacheUserHistoryCollectionIntraday,
            UserMtgaDeckRepository cacheUserAllMtgaDecks,
            UserCollectionFetcher userCollectionFetcher,
            IQueryHandler<MtgaDecksFoundQuery, HashSet<string>> qDecksFound)
        {
            this.mapper = mapper;
            AllCards = cardRepo;
            this.ConfigApp = configApp;
            this.ConfigDecks = configDecks;
            this.ConfigUsers = configUsers;
            //this.loaderCards = loaderCards;
            this.comparer = comparer;
            this.deckConverter = deckConverter;
            this.writerDeckMtga = writerDeckMtga;
            this.userManager = userManager;
            //this.matchesCacheManager = matchesCacheManager;
            this.activeUserCounter = activeUserCounter;

            this.cacheUserHistoryMtgaDecksFound = cacheUserHistoryMtgaDecksFound;
            //this.cacheUserHistoryCollectionIntraday = cacheUserHistoryCollectionIntraday.Init(this.ConfigApp.FolderDataConfigUsers);
            this.cacheUserAllMtgaDecks = cacheUserAllMtgaDecks;

            this.rawDeckConverter = rawDeckConverter;

            this.userCollectionFetcher = userCollectionFetcher;
            this.qDecksFound = qDecksFound;

            //Log.Information($"Config for Decks: {ConfigDecks.Get().Count} entries");
        }

        public UserStatusEnum GetUserStatus(string userId)
        {
            UserStatusEnum result;
            if (ConfigUsers.Get(userId) != null)
                result = UserStatusEnum.InMemory;
            else
            {
                if (File.Exists(Path.Combine(ConfigApp.FolderDataConfigUsers, userId, $"{userId}_userconfig.json")))
                    result = UserStatusEnum.OnDisk;
                else
                    result = UserStatusEnum.NonExistent;
            }

            Log.Debug("GetUserStatus({userId}): {status}", userId, result.ToString());
            return result;
        }

        public async Task<Dictionary<string, ConfigModelDeck>> GetDecksIncludingUserCustom(string userId, bool filterWithScrapersActive = true)
        {
            var systemDecks = ConfigDecks.Get()
               .Where(i => i.Deck != null)   // Added for comparisons when server just started and is still loading decks
               ;

            if (userId == null)
                return systemDecks.ToDictionary(i => i.Id, i => i);

            var configUser = ConfigUsers.Get(userId);
            var res = new Dictionary<string, ConfigModelDeck>();

            //var test = CardsContainer.decks
            //    .Where(i => ConfigDecks.Get(i.Key) == null)
            //    .ToArray();

            //var t1 = CardsContainer.decks.First(i => i.Value.Name == "20190303 - Bant Emergency");
            //var t3 = t1.Value.Id;
            //var t2 = ConfigDecks.Get().First(i => i.Name == "20190303 - Bant Emergency");

            //var mtgaDecks = configUser.DataInMemory.GetMtgaDecks().Where(i => configUser.DataInMemory.HistoryDetails.GetLastMtgaDecksFound().Info.Contains(i.Id));
            var userDecksFound = await qDecksFound.Handle(new MtgaDecksFoundQuery(userId));
            var mtgaDecks = (await cacheUserAllMtgaDecks.Get(userId))
                .Where(i => userDecksFound.Contains(i.Id));

            var mtgaDecksMapped = mtgaDecks
                //.Where(i => configUser.DataInMemory.HistoryDetails.GetLastMtgaDecksFound().Info.Contains(i.Id))
                //.Where(i => i.CardsMain.Any(x => allCards[x.Key].set == ConfigApp.CurrentSet))
                .Select(i =>
                {
                    var deck = mapper.Map<ConfigModelDeck>(i);
                    deck.Deck = new Deck(i.Name, new ScraperType(Entity.Constants.USERDECK_SOURCE_MTGADECK), i.ToDeckCards(AllCards));
                    deck.Id = deck.Deck.Id;
                    return deck;
                })
                .GroupBy(i => i.Id)
                .Select(i => i.Last())
                .ToArray();

            systemDecks = systemDecks.Union(mtgaDecksMapped);

            if (filterWithScrapersActive)
                systemDecks = systemDecks.Where(i => configUser.ScrapersActive.Contains(i.ScraperTypeId));

            foreach (var configDeck in systemDecks)
                res.Add(configDeck.Id, configDeck);

            lock (lockDecksUserDefined)
            {
                foreach (var configDeck in configUser.DataInMemory.DecksUserDefined.Where(i => res.ContainsKey(i.Key) == false))
                    res.Add(configDeck.Key, configUser.CustomDecks[configDeck.Key]);
            }

            return res;
        }

        //private void LoadDeckUserCustomFromConfigAndFile(string userId, ConfigModelDeck configDeck, ICollection<DeckCard> cards)
        //{
        //    userDataByUserId[userId].DecksUserDefined[configDeck.Id] = new Deck(configDeck.Name, new ScraperType(ScraperTypeEnum.UserCustomDeck, userId).Id, cards);
        //    Log.Debug("Custom deck {customDeckName} ({customDeckId}) loaded for user {userId}", configDeck.Name, configDeck.Id, userId);
        //}

        //public CardsContainerInitResultEnum ReloadMasterData(bool loadDecks)
        //{
        //    Log.Information("ReloadMasterData");

        //    var initResult = loaderCards.InitWithoutLoadingDecks();

        //    if (loadDecks)
        //        //containerLoader.LoadDecks(ConfigApp.FolderDataDecks);
        //        loaderDecks.LoadDecks();

        //    if (initResult == CardsContainerInitResultEnum.Success)
        //    {
        //        deckConverter.Init(allCards);

        //        //if (loadDecks)
        //        //    ConfigDecks.SyncWithDecks(CardsContainer.decks);
        //    }

        //    rawDeckConverter = new RawDeckConverter().Init(allCards);
        //    userManager.Init(allCards);

        //    return initResult;
        //}

        //public SessionContainerUserModel GetUserSession(string userId)
        //{
        //    lock (lockUsers)
        //    {
        //        if (userDataByUserId.ContainsKey(userId) == false)
        //        {
        //            Log.Warning("GetUserSession - User {userId} was unloaded from memory, reloading it.", userId);

        //            return null;
        //            //LoadUserIfNeeded(userId);
        //        }

        public async Task SetUserCollection(string userId, OutputLogResult outputLogResult)
        {
            await userManager.SaveNewInfo(userId, outputLogResult);
            Compare(userId);
        }

        public void SaveToDatabase(string userId, string email, OutputLogResult2 outputLogResult2)
        {
            // To reactivate later maybe
            //if (configReloadable.CurrentValue.Features.ContainsKey("SqlDatabase") && configReloadable.CurrentValue.Features["SqlDatabase"])
            //{
            //    try
            //    {
            //        var watch = Stopwatch.StartNew();
            //        databaseAdapter.SaveNewInfoFromEmail(email, outputLogResult2);
            //        watch.Stop();
            //        Log.Information("Saving to database took {ms}ms", watch.ElapsedMilliseconds);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Error(ex, "SaveToDatabase Error (user {userId} {email}):", userId, email);
            //    }
            //}
        }

        //        return userDataByUserId[userId];
        //    }
        //}

        //public (DateTime collectionDate, ICollection<CardWithAmount> cards, MtgaUserProfile profile) GetUserCollection(string userId)
        //{
        //    var configUser = ConfigUsers.Get(userId);
        //    return (configUser.CollectionDate, configUser.DataInMemory.Collection, configUser.MtgaUserProfile);
        //}

        public Dictionary<string, DashboardModelSummary> GetUserDashboardSummary(string userId)
        {
            var res = Compare(userId);
            var details = res.GetModelDetails();
            var detailsBySet = details.GroupBy(i => i.Set).ToDictionary(i => i.Key, i => i.ToArray());

            var evMysticalArchive = detailsBySet.ContainsKey("STA") ? GetExpectedValue(detailsBySet["STA"]) : 0;

            //return res.GetModelSummary(true).ToDictionary(i => i.Key, i => new DashboardModelSummary
            //{
            //    Data = i.Value,
            //    ExpectedValue = GetExpectedValue(detailsBySet[i.Key]),
            //});

            var summary = res.GetModelSummary(true);
            var result = summary.ToDictionary(i => i.Key, i => new DashboardModelSummary
            {
                Data = i.Value,
                ExpectedValue = GetExpectedValue(detailsBySet.ContainsKey(i.Key) ? detailsBySet[i.Key] : Array.Empty<CardMissingDetailsModel>()),
                ExpectedValueOther = i.Key == "STX" ? new KeyValuePair<string, float>("Mystical Archive (STA)", evMysticalArchive) : default,
            });
            return result;
        }

        public CardMissingDetailsModel[] GetUserDashboardDetails(string userId)
        {
            var res = Compare(userId);
            var details = res.GetModelDetails();
            return details.Where(i => i.NbMissing > 0).ToArray();
        }

        private float GetExpectedValue(ICollection<CardMissingDetailsModel> data)
        {
            var missing = data
                .Where(i => i.Rarity == RarityEnum.Mythic || i.Rarity == RarityEnum.Rare || i.Rarity == RarityEnum.RareLand || i.Rarity == RarityEnum.RareNonLand)
                .Where(i => i.NbOwned != 4);

            var nbMissing = missing.Count();

            var sum = missing
                .Where(i => i.NbMissing > 0)
                .Sum(i =>
                {
                    var mythicProbability = i.Set switch
                    {
                        // https://magic.wizards.com/en/promotions/drop-rates
                        // https://magic.wizards.com/en/articles/archive/making-magic/set-boosters-2020-07-25
                        "AKR" => 1 / 6f,
                        "KLR" => 1 / 7f,
                        //"M21" or "IKO" or "THB" or "ELD" or "M20" or "WAR" or "RNA" or "GRN" or "M19" or "DOM" or "RIX" or "XLN" => 1 / 8f,
                        "M21" => 1 / 8f,
                        "IKO" => 1 / 8f,
                        "THB" => 1 / 8f,
                        "ELD" => 1 / 8f,
                        "M20" => 1 / 8f,
                        "WAR" => 1 / 8f,
                        "RNA" => 1 / 8f,
                        "GRN" => 1 / 8f,
                        "M19" => 1 / 8f,
                        "DOM" => 1 / 8f,
                        "RIX" => 1 / 8f,
                        "XLN" => 1 / 8f,
                        _ => 1 / 7.4f,
                    };

                    var step1 = i.MissingWeight / i.NbMissing;
                    var rarityProbability = i.Rarity == RarityEnum.Mythic ? mythicProbability : 1 - mythicProbability;

                    return step1 * rarityProbability;
                });

            return sum / nbMissing;
        }

        public async Task<bool> RegisterUser(string userId, string referer = null)
        {
            Log.Debug("RegisterUser({userId})", userId);
            var configUser = await ConfigUsers.LoadUserIfNotSet(userId, () => userManager.LoadUser(userId, referer));
            
            var sw = Stopwatch.StartNew();
            bool doCompare;
            lock (lockDecksUserDefined)
            {
                doCompare = configUser.DataInMemory.CompareResult.ByCard.Count == 0;
            }

            if (doCompare)
                Compare(userId);
            sw.Stop();
            Log.Debug("SW4 {sw1} s", (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            sw = Stopwatch.StartNew();
            await ConfigUsers.MutateUser(userId).CountLogin(DateTime.UtcNow);
            sw.Stop();
            Log.Debug("SW5 {sw1} s", (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            var changesSinceLastLogin = configUser.LastLoginUtc < GetVersionUtc();
            return changesSinceLastLogin;
        }

        public CardsMissingResult Compare(string userId)
        {
            lock (lockDecksUserDefined)
            {
                var configUser = ConfigUsers.Get(userId);
                //var caller = new StackFrame(1, true).GetMethod().Name;

                var lastCompare = (DateTime.UtcNow - configUser.DataInMemory.LastCompareUtc).TotalMilliseconds;
                if (lastCompare < 10000d)
                {
                    //Log.Debug("Skipping Comparison request from {caller} for {userId} - Last compare: {lastCompare}ms ago", caller, userId, (int)lastCompare);
                    return configUser.DataInMemory.CompareResult;
                }

                configUser.DataInMemory.LastCompareUtc = DateTime.UtcNow;
                //Log.Debug("Comparison requested from {caller} for user {userId}", caller, userId);
                activeUserCounter.SetActive(userId);

                //var collection = configUser.DataInMemory.HistoryDetails.GetLastCollectionInMemory(rawDeckConverter).Info;
                var collectionData = userCollectionFetcher.GetLatestCollection(userId).Result;
                var collection = rawDeckConverter.LoadCollection(collectionData.Info);

                var decks = ConfigDecks.Get().Select(i => i.Deck).ToArray();

                //var mtgaDecks = configUser.DataInMemory.GetMtgaDecks()
                //    .Where(i => configUser.DataInMemory.HistoryDetails.GetLastMtgaDecksFound().Info.Contains(i.Id));
                var userDecksFound = qDecksFound.Handle(new MtgaDecksFoundQuery(userId)).Result;
                var mtgaDecks = cacheUserAllMtgaDecks.Get(userId)
                    .Result
                    .Where(i => userDecksFound.Contains(i.Id));

                var decksMtgaMapped = mtgaDecks
                    //.Where(i => i.CardsMain.Any(x => allCards[x.Key].set == ConfigApp.CurrentSet))
                    .Select(
                        i => new Deck(
                            i.Name,
                            new ScraperType(Entity.Constants.USERDECK_SOURCE_MTGADECK),
                            i.ToDeckCards(AllCards)))
                    .ToArray();

                var result = comparer.Init(configUser, collection)
                    .Compare(decks, configUser.DataInMemory.DecksUserDefined.Values, decksMtgaMapped);
                configUser.DataInMemory.CompareResult = result;
                return result;
            }
        }

        //public ICollection<UserInfo> GetUsers(bool filterActive)
        //{
        //    IEnumerable<UserInfo> info = null;

        //    lock (lockDecksUserDefined)
        //    {
        //        info = ConfigUsers.Values
        //           .Select(i => {
        //                var configUser = ConfigUsers.Get(i.Id);
        //                //return new UserInfo(configUser)
        //                //{
        //                //    CollectionSizeInMemory = i.Value.Collection.Sum(c => c.Amount),
        //                //    NbDecksMonitored = GetDecksIncludingUserCustom(i.Key).Count,
        //                //    LastLoginHours = (DateTime.Now - configUser.LastLogin).TotalHours,
        //                //    CollectionOnDisk = File.Exists(Path.Combine(ConfigUsers.FolderConfigUser, $"{configUser.Id}_collection.txt")),
        //                //};
        //                var res = UserInfo.FromConfigModelUser(configUser)
        //                    .With(
        //                        //i.DataInMemory.HistoryDetails.GetLastCollection().Info.Sum(c => c.Value),
        //                        GetDecksIncludingUserCustom(i.Id).Count,
        //                        (DateTime.UtcNow - configUser.LastLoginUtc).TotalHours
        //                        //, i.DataInMemory.HistoryDetails.CollectionByDate.GetData().Any()
        //                    );

        //               return res;
        //           });
        //    }

        //    var filtered = info;

        //    if (filterActive)
        //        filtered = filtered.Where(i => i.CollectionOnDisk);//.Where(i => i.NbLogin > 5 && (DateTime.Now - i.LastLogin).TotalHours <= 24);

        //    return filtered.OrderBy(i => i.LastLoginHours).ToArray();
        //}

        public (int totalDecks, ICollection<DeckSummary> decks) GetSystemDecks(string userId, FilterDeckParams filters)
        {
            var allDecks = ConfigDecks.Get();
            var totalDecks = allDecks.Count;

            var decksFiltered = allDecks
                .Where(i => i.Deck != null)
                .Where(i => i.Deck.FilterSource(i.ScraperTypeId, filters.ScraperTypeId))
                .Where(i => i.Deck.FilterColor(filters.Color))
                .Where(i => i.Deck.FilterName(filters.Name))
                .Where(i => i.Deck.FilterCard(filters.Card))
                .OrderByDescending(i => i.DateCreatedUtc)
                .ThenBy(i => i.ScraperTypeId)
                .ThenBy(i => i.Name);

            var systemDecks = mapper.Map<ICollection<DeckSummary>>(decksFiltered);
            return (totalDecks, systemDecks);
        }

        public async Task<(int totalDecks, ICollection<DeckTrackedSummary> decks)> GetDecksTracked(string userId, FilterDeckParams filters)
        {
            Func<UserDataInMemoryModel, string, float> GetUserMissingWeightForDeck = (ud, deckId) =>
            {
                //if (GetDecksIncludingUserCustom(userId)[deckId].Name == "Azorius High Alert")
                //    System.Diagnostics.Debugger.Break();

                var compareResult = ud.CompareResult;
                if (compareResult.ByDeck.ContainsKey(deckId) == false)
                {
                    //Log.Debug("Deck {deckId} not found in comparison result for user {userId}", deckId, uId);
                    return 0f;
                }

                return compareResult.ByDeck[deckId].MissingWeight;
            };

            StopwatchOperation("Compare", () => Compare(userId));

            var decksToCheck = await StopwatchOperation("decksToCheck", () => GetDecksIncludingUserCustom(userId));

            //try
            //{
            lock (lockDecksUserDefined)
            {
                var userData = ConfigUsers.Get(userId).DataInMemory;
                var decks = StopwatchOperation("decksCalc", () => decksToCheck.Values
                    .Where(i => i.Deck.FilterSource(i.ScraperTypeId, filters.ScraperTypeId))
                    .Where(i => i.Deck.FilterColor(filters.Color))
                    .Where(i => i.Deck.FilterName(filters.Name))
                    .Where(i => i.Deck.FilterCard(filters.Card))
                    //.Where(i => i.Value.FilterDate())
                    .Select(configDeck =>
                    {
                        var missingWeight = GetUserMissingWeightForDeck(ConfigUsers.Get(userId).DataInMemory, configDeck.Id);
                        var priorityFactor = ConfigUsers.Get(userId).PriorityByDeckId.ContainsKey(configDeck.Id) == true ? ConfigUsers.Get(userId).PriorityByDeckId[configDeck.Id] : 1.0f;

                        var deckSummary = mapper.Map<DeckTrackedSummary>(configDeck);
                        deckSummary.MissingWeight = missingWeight;
                        deckSummary.MissingWeightBase = priorityFactor == 0 ? missingWeight : missingWeight / priorityFactor;
                        deckSummary.PriorityFactor = priorityFactor;
                        deckSummary.WildcardsMissingMain = userData.CompareResult.GetWildcardsMissing(configDeck.Id, false, true);
                        deckSummary.WildcardsMissingSideboard = userData.CompareResult.GetWildcardsMissing(configDeck.Id, true, true);

                        if (deckSummary.WildcardsMissingMain.Count == 0)
                            Log.Warning("Deck <{deckName}> glitched for {userId}", deckSummary.Name, userId);

                        //if (userData.CompareResult.ByDeck.ContainsKey(configDeck.Id) == false)
                        //{
                        //    //var decksWithSameName = userData.CompareResult.ByDeck.Where(i => i.Value.)
                        //    Log.Error(
                        //        "{userId} GetDecksTracked Error: Deck Id '{deckId}' ({deckName}) not found in userData.CompareResult.ByDeck...recalculated ID: {newDeckId}",
                        //        userId, configDeck.Id, configDeck.Name, configDeck.Deck.GetId());
                        //}

                        return deckSummary;
                    })

                    //FILTER OUT THOSE WITH THAT WEIRD GLITCH
                    .Where(i => i.WildcardsMissingMain.Count > 0)

                    .OrderBy(i => i.MissingWeight)
                    .ThenBy(i => i.WildcardsMissingMain.Sum(x => Convert.ToInt32(x.Key) * x.Value) + i.WildcardsMissingSideboard.Sum(x => Convert.ToInt32(x.Key) * x.Value))
                    .ThenBy(i => i.Name)
                    .ToArray()
                );

                return (decksToCheck.Count, decks);
            }
            //}
            //catch (Exception ex)
            //{
            //    Debugger.Break();
            //    throw;
            //}
        }

        public async Task<DeckTrackedDetails> GetDeck(string userId, string deckId)
        {
            Log.Information("GetDeck({userId}, {deckId})", userId, deckId);

            ConfigModelDeck configDeck = null;
            var allDecks = await GetDecksIncludingUserCustom(userId, false);
            var configUser = ConfigUsers.Get(userId);

            if (userId == null)
            {
                Log.Warning("GetDeck {deckId} user is NULL", deckId);
                return null;
            }
            
            if (allDecks.ContainsKey(deckId) == false)
            {
                // Special case when requesting a deck from the tracker Possible deck from Opponent
                var key = allDecks.Keys.FirstOrDefault(x => Fnv1aHasher.To32BitFnv1aHash(x) == Convert.ToUInt32(deckId));
                if (key == null)
                {
                    //// Special case when requesting a deck from someone else
                    //// (only case should be a user custom deck shared to someone else)
                    //configDeck = ConfigDecks.Get().FirstOrDefault(i => i.Id == deckId);
                    //if (configDeck != null)
                    //{
                    //    var configCopy = JsonConvert.DeserializeObject<IImmutableUser>(JsonConvert.SerializeObject(configUser));
                    //    configCopy.ScrapersActive = new List<string>();
                    //    var systemDeck = configDeck.Deck;
                    //    var tempCompare = comparer.Init(configCopy, configUser.DataInMemory.Collection).Compare(new[] { systemDeck }, new IDeck[0]);
                    //    return GetDeckInfo(configCopy, configDeck, tempCompare);
                    //}
                    //else
                    //{
                    Log.Error("User {userId} tried to get deck {deckId} and it was not found", userId, deckId);
                    return null;
                    //}
                }
                else
                    deckId = allDecks[key].Id;
            }

            configDeck = allDecks[deckId];

            var results = new CardsMissingResult { };
            var decksTracked = new Dictionary<string, bool> { { deckId, configUser.IsTracked(configDeck.Deck) } };

            results.ByDeck[deckId] = new CardRequiredInfoByDeck(deckId, Array.Empty<CardRequiredInfo>(), decksTracked);

            lock (lockDecksUserDefined)
            {
                if (configUser.DataInMemory.CompareResult.ByDeck.ContainsKey(deckId))
                    results = configUser.DataInMemory.CompareResult;
                else if (configUser.PriorityByDeckId.ContainsKey(deckId) && configUser.PriorityByDeckId[deckId] != 0)
                {
                    var deckScraper = configDeck.ScraperTypeId;
                    var scrapersActive = string.Join(", ", ConfigUsers.Get(userId).ScrapersActive);
                    Log.Error("No CompareResult found for GetDeck(userId:{userId}, deckId:{deckId}) Priority: {priority}{NewLine}deckScraper:{deckScraper}, scrapersActive:{scrapersActive}",
                        userId, deckId, configUser.PriorityByDeckId[deckId], deckScraper, scrapersActive);
                }

                {
                    //var collection = configUser.DataInMemory.HistoryDetails.GetLastCollectionInMemory(rawDeckConverter).Info;
                    var collectionData = userCollectionFetcher.GetLatestCollection(userId).Result;
                    var collection = rawDeckConverter.LoadCollection(collectionData.Info);

                    writerDeckMtga.Init(collection, configUser.LandsPreference, configUser.LandsPickAll);
                }
            }

            return GetDeckInfo(configUser, configDeck, results);
        }

        public async Task<DeckTrackedDetails> GetDeckByHash(string userId, uint hash)
        {
            var deckId = ConfigDecks.Get().FirstOrDefault(i => Fnv1aHasher.To32BitFnv1aHash(i.Id) == hash)?.Id;
            if (deckId != null)
            {
                Log.Information("GetDeckByHash({userId}, {hash}) System deck found: {deckId}", userId, hash, deckId);
                return await GetDeck(userId, deckId);
            }

            var loadedUserDecks = ConfigUsers.Values.SelectMany(i => i.CustomDecks.Values).ToArray();
            deckId = loadedUserDecks.FirstOrDefault(i => Fnv1aHasher.To32BitFnv1aHash(i.Id) == hash)?.Id;
            if (deckId != null)
            {
                Log.Information("GetDeckByHash({userId}, {hash}) User deck found in memory: {deckId}", userId, hash, deckId);
                return await GetDeck(userId, deckId);
            }

            //var userDecksOnDisk = Directory.GetFiles(ConfigApp.FolderDataConfigUsers, "*.json", SearchOption.TopDirectoryOnly);
            //int idx = 0;
            //while (deckId == null && idx < userDecksOnDisk.Length)
            //{
            //    var filepath = userDecksOnDisk[idx];
            //    LogExt.LogReadFile(filepath, userId);
            //    var configUser = JsonConvert.DeserializeObject<IImmutableUser>(File.ReadAllText(filepath));
            //    var configDeck = configUser.CustomDecks.Values.FirstOrDefault(i => util.To32BitFnv1aHash(i.Id) == hash);

            //    if (configDeck != null)
            //    {
            //        Log.Warning("GetDeckByHash({userId}, {hash}) User deck found on disk: {deckId}", userId, hash, deckId);

            //        var cardsMain = configDeck.CardsMain.Select(i => new DeckCard(
            //            new CardWithAmount(allCards.First(x => x.grpId == i.Key), i.Value), false));
            //        var cardsSideboard = configDeck.CardsSideboard.Select(i => new DeckCard(
            //            new CardWithAmount(allCards.First(x => x.grpId == i.Key), i.Value), true));

            //        configDeck.Deck = new Deck(configDeck.Name, new ScraperType(configDeck.ScraperTypeId), cardsMain.Union(cardsSideboard).ToArray());

            //        lock (lockDecksUserDefined)
            //        {
            //            var collection = configUser.DataInMemory.HistoryDetails.GetLastCollectionInMemory(rawDeckConverter).Info;
            //            var compareResult = comparer.Init(configUser, collection).Compare(new[] { configDeck.Deck }, new Deck[] { });
            //            return GetDeckInfo(configUser, configDeck, compareResult);
            //        }
            //    }

            //    idx++;
            //}

            // Deck NOT FOUND
            return null;
        }

        private DeckTrackedDetails GetDeckInfo(IImmutableUser configUser, ConfigModelDeck configDeck, CardsMissingResult results)
        {
            var mtgaImportFormat = writerDeckMtga.ToText(configDeck.Deck);
            CardsMissingResult compareResult = null;

            lock (lockDecksUserDefined)
            {
                //var collection = configUser.DataInMemory.HistoryDetails.GetLastCollectionInMemory(rawDeckConverter).Info;
                var collectionData = userCollectionFetcher.GetLatestCollection(configUser.Id).Result;
                var collection = rawDeckConverter.LoadCollection(collectionData.Info);

                compareResult = comparer.Init(configUser, collection).Compare(new[] { configDeck.Deck }, Array.Empty<Deck>(), Array.Empty<Deck>());
            }

            var deckToShow = new Deck(configDeck.Name, new ScraperType(configDeck.ScraperTypeId), deckConverter.Convert(configDeck.Name, mtgaImportFormat));

            return new DeckTrackedDetails
            {
                OriginalDeckId = configDeck.Id,
                Deck = deckToShow,
                CardsRequired = results.ByDeck[configDeck.Id],
                Config = configDeck,
                MtgaImportFormat = mtgaImportFormat,
                CompareResult = compareResult
            };
        }

        //public void AddDecks(ICollection<ConfigModelDeck> decks)
        //{
        //    Log.Information($"AddDecks: {decks.Count} decks");

        //    if (decks.Count > 0)
        //    {
        //        foreach (var d in decks)
        //            ConfigDecks.Set(d);

        //        ConfigDecks.Save();
        //    }
        //}

        public async Task SetDeckPriorityFactor(string userId, string deckId, float value, bool saveConfig)
        {
            //Log.Information("SetDeckPriorityFactor({userId}, {deckId}, {value}, {saveConfig})", userId, deckId, value, saveConfig);

            var userConfig = ConfigUsers.Get(userId);
            if (userConfig == null || (await GetDecksIncludingUserCustom(userId)).ContainsKey(deckId) == false)
            {
                Log.Error("To check for User {userId}: SetDeckPriorityFactor: userConfig == null", userId);
                return;
            }

            await ConfigUsers.MutateUser(userId).SetDeckPriorityFactor(deckId, value);
        }
        
        public async Task DeckPriorityFactorResetAll(string userId, float value)
        {
            IEnumerable<string> decksToReset = (await GetDecksIncludingUserCustom(userId)).Keys;
            var configUser = ConfigUsers.Get(userId);

            // When reseting to a value tracking (not 0),
            // only reset values for decks not tracked (don't alter already tracked decks)
            if (value != 0)
                decksToReset = decksToReset
                    .Where(d => configUser.PriorityByDeckId.ContainsKey(d))
                    .Where(d => configUser.PriorityByDeckId[d] == 0);

            var changes = decksToReset.Select(d => KeyValuePair.Create(d, value)).ToArray();

            Log.Information("DeckPriorityFactorResetAll({userId}, {value}) {nbDecks} decks", userId, value, changes.Length);
            await ConfigUsers.MutateUser(userId).SetDeckPriorityFactors(changes);
            //Compare(userId);
        }


        /// <param name="userId">the user to update</param>
        /// <param name="doTrack">
        /// false: Remove passed decks from tracked decks;
        /// true: Add passed decks to tracked decks;
        /// null: Track ONLY the passed decks
        /// </param>
        /// <param name="deckIds">the ids of the decks to update priority for</param>
        public async Task SetDeckPriorityFactorForFilteredDecks(string userId, bool? doTrack, ICollection<string> deckIds)
        {
            var configUser = ConfigUsers.Get(userId);
            Log.Information("SetDeckPriorityFactorForFilteredDecks({userId}, {doTrack}) {nbDecks} decks", userId, doTrack?.ToString() ?? "(null)", deckIds.Count);

            var decksIncludingCustom = (await GetDecksIncludingUserCustom(userId)).Keys;
            IMutateUser mutate = ConfigUsers.MutateUser(userId); 
            if (doTrack == false)
            {
                await mutate.SetDeckPriorityFactors(
                    decksIncludingCustom
                        .Where(deckIds.Contains)
                        .Select(d => KeyValuePair.Create(d, 0f)));
                return;
            }

            var keepPrevValue = configUser.PriorityByDeckId
                .Where(kvp => kvp.Value > 0 && kvp.Value != 1f)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            if (doTrack == true)
            {
                // true: Add filtered decks to tracked decks
                await mutate.SetDeckPriorityFactors(
                    decksIncludingCustom
                        .Where(deckIds.Contains)
                        .Select(d =>
                        {
                            var v = keepPrevValue.GetValueOrDefault(d, 1f);
                            return KeyValuePair.Create(d, v);
                        }));
            }
            else
            {
                // null: Track ONLY the filtered decks
                Log.Information("{userId}: doTrack is null, setting priority 0 to some decks", userId);
                await mutate.SetDeckPriorityFactors(
                    decksIncludingCustom
                        .Select(d =>
                        {
                            var v = deckIds.Contains(d)
                                ? keepPrevValue.GetValueOrDefault(d, 1f)
                                : 0f;
                            return KeyValuePair.Create(d, v);
                        }));
            }
        }

        public async Task<string> AddCustomDeck(string userId, string deckName, string url, string mtgaFormat)
        {
            Log.Information("AddCustomDeck({userId}, {name}, {url})", userId, deckName, string.IsNullOrEmpty(url) ? "(no url)" : url);

            var scraperType = new ScraperType(ScraperTypeEnum.UserCustomDeck, userId);

            var deck = new Deck(deckName, scraperType, deckConverter.Convert(deckName, mtgaFormat));
            var deckId = deck.Id;

            var configUser = ConfigUsers.Get(userId);
            lock (lockDecksUserDefined)
            {
                configUser.DataInMemory.DecksUserDefined[deckId] = deck;
            }

            var configModelDeck = new ConfigModelDeck(deck, url, DateTime.UtcNow)
            {
                //CardsMain = deck.Cards.QuickCardsMain.ToDictionary(i => i.Key, i => i.Value.Amount),
                //CardsSideboard = deck.Cards.QuickCardsSideboard.ToDictionary(i => i.Key, i => i.Value.Amount),
                Cards = deck.Cards.All.Select(i => new DeckCardRaw
                {
                    GrpId = i.Card.GrpId,
                    Amount = i.Amount,
                    Zone = i.Zone
                }).ToArray()
            };
            //configUser.PriorityByDeckId[deckId] = 1.0f;
            await ConfigUsers.MutateUser(userId).AddCustomDeck(deckId, configModelDeck);

            //writerDeck.Save(new Deck(deckId, scraperType, deck.Cards.All), Path.Combine(ConfigApp.FolderDataDecks, ConfigModelDeck.SOURCE_USERCUSTOM));

            Log.Information("User {userId} imported custom deck {name} successfully ({deckId})", userId, deckName, deckId);
            return deckId;
        }

        public async Task<bool> DeleteCustomDeck(string userId, string deckId, bool isUserCustomDeck)
        {
            Log.Debug("DeleteCustomDeck({userId}, {deckId})", userId, deckId);

            var configUser = ConfigUsers.Get(userId);

            lock (lockDecksUserDefined)
            {
                if (configUser.DataInMemory.DecksUserDefined.ContainsKey(deckId) == false)
                {
                    Log.Error("User {userId} tried to delete custom deck {deckId} but it was not found", userId, deckId);
                    return false;
                }

                configUser.DataInMemory.DecksUserDefined.Remove(deckId);
            }

            if (isUserCustomDeck)
            {
                await ConfigUsers.MutateUser(userId).TryRemoveCustomDeck(deckId);
            }

            Log.Information("User {userId} deleted custom deck {deckId} successfully", userId, deckId);
            return true;
        }

        //public bool DeleteCustomScraper(string userId, ScraperType scraperType)
        //{
        //    Log.Information("DeleteCustomScraper({userId}, {scraperType})", userId, scraperType.Id);
        //    var configUser = ConfigUsers.Get(userId);

        //    var scraperFound = configUser.ScrapersActive.FirstOrDefault(i => i == scraperType.Id);

        //    // Validation that everything is in the right set for the delete to be possible
        //    if (scraperType.Type != ScraperTypeEnum.Streamdecker ||
        //        userDataByUserId.ContainsKey(userId) == false ||
        //        configUser == null ||
        //        scraperFound == null)
        //    {
        //        logWithGmail.Error("User {userId} tried to delete custom scraper {scraperType} but it was not found", userId, scraperType.Id);
        //        return false;
        //    }

        //    //foreach (var d in config.CustomDecks.Where(i => i.Value.Source == source))
        //    //    DeleteCustomDeck(userId, d.Key, false);

        //    configUser.ScrapersActive.Remove(scraperFound);
        //    ConfigUsers.Save();

        //    //Compare(userId);

        //    userDataByUserId[userId].DecksUserDefined = new Dictionary<string, Deck>();
        //    LoadUserValue(configUser);

        //    Log.Information("User {userId} deleted custom scraper {type} {name} successfully", userId, scraperType.Id);
        //    return true;
        //}

        public async Task SaveUserPreference(string userId, UserPreferenceEnum preferenceKey, string value)
        {
            Log.Information("SaveUserPreference({userId}, {preferenceKey}, {value}", userId, preferenceKey, value);

            switch (preferenceKey)
            {
                case UserPreferenceEnum.ThemeIsDark:
                    await ConfigUsers.MutateUser(userId).SetPreferenceThemeIsDark(Convert.ToBoolean(value));
                    break;

                case UserPreferenceEnum.CollectionSetsOrder:
                    await ConfigUsers.MutateUser(userId).SetPreferenceCollectionSetsOrder(value);
                    break;

                case UserPreferenceEnum.LandsPickAll:
                    await ConfigUsers.MutateUser(userId).SetPreferenceLandsPickAll(Convert.ToBoolean(value));
                    break;
            }
        }

        public Dictionary<string, string> GetUserPreferences(string userId)
        {
            var userConfig = ConfigUsers.Get(userId);
            return new Dictionary<string, string>
            {
                { UserPreferenceEnum.ThemeIsDark.ToString(), userConfig == null ? true.ToString() : userConfig.ThemeIsDark.ToString() },
                { UserPreferenceEnum.CollectionSetsOrder.ToString(), userConfig == null ? "NewestFirst" : userConfig.CollectionSetsOrder },
                { UserPreferenceEnum.LandsPickAll.ToString(), userConfig == null ? false.ToString() : userConfig.LandsPickAll.ToString() },
            };
        }

        public async Task<ICollection<ScraperDto>> GetScrapersInfo(string userId)
        {
            var configUser = ConfigUsers.Get(userId);

            //var mtgaDecks = configUser.DataInMemory.GetMtgaDecks()
            //    .Where(i => configUser.DataInMemory.HistoryDetails.GetLastMtgaDecksFound().Info.Contains(i.Id))
            //    .ToList();
            var mtgaDecks = await qDecksFound.Handle(new MtgaDecksFoundQuery(userId));

            var sourceMtgaDecks = new ScraperDto(
                new ScraperType(Entity.Constants.USERDECK_SOURCE_MTGADECK),
                configUser.ScrapersActive.Contains(Entity.Constants.USERDECK_SOURCE_MTGADECK),
                mtgaDecks.Count,
                "");

            var decksGroupedByScraperType = ConfigDecks.Get()
                .OrderBy(i => i.ScraperTypeId)
                .GroupBy(i => i.ScraperTypeId);

            var res = decksGroupedByScraperType
                .Select(i => new ScraperDto(new ScraperType(i.Key), configUser.ScrapersActive.Contains(i.Key), i.Count(), i.First().UrlDeckList))
                .Append(sourceMtgaDecks)
                .ToArray();

            return res;
        }

        public async Task ActivateScraper(string userId, string scraperTypeId, bool activate)
        {
            Log.Information("ActivateScraper({userId}, {scraperTypeId}, {activate})", userId, scraperTypeId, activate);

            if (activate)
            {
                await ConfigUsers.MutateUser(userId).AddScraperActive(scraperTypeId);
            }
            else
            {
                await ConfigUsers.MutateUser(userId).RemoveScraperActive(scraperTypeId);
            }
        }

        public async Task UpdateScrapersActive(string userId, ICollection<string> scrapersActive)
        {
            var strScrapersActive = JsonConvert.SerializeObject(scrapersActive);
            Log.Information("UpdateScrapers({userId}, {scrapersActive})", userId, strScrapersActive);

            var configUser = ConfigUsers.Get(userId);
            if (configUser == null)
                return;

            await ConfigUsers.MutateUser(userId).PutScrapersActive(scrapersActive);
        }

        public Dictionary<(ScraperTypeEnum, ScraperTypeFormatEnum), ICollection<string>> GetScraperUsers()
        {
            //var manualStreamdeckers = new[]
            //{
            //    "jeffhoogland",
            //    "deezy_mtg",
            //    "ildelmo",
            //    "magicshibby",
            //    "channelfireball",
            //    "crokeyz",
            //    "legenvd",
            //    "sandoiche",
            //    "lucaparroz",
            //    "mtgarenazone",
            //    "sidetrakisbad",
            //    "hueywj",
            //    "predimtg",
            //    "tigerzord",
            //    "alieldrazi",
            //    "danytlaw",
            //    "martypunker",
            //    "ccalcano",
            //    "mystmin",
            //    "sol4r1s",
            //    "loxodont",
            //    "benjamin_wheeler",
            //    "sethmanfieldmtg",
            //    "sorquixe",
            //    "ashlizzlle",
            //    "ziogarbe",
            //    "rintms",
            //    "h0lydiva",
            //    "kanister_mtg",
            //    "ondrejstrasky",
            //    "slimo0",
            //    "thaeyn",
            //    "simoneakiratrimarchi",
            //    "martinjuza",
            //    "calebdmtg",
            //    "filipacarola",
            //    "krowz",
            //    "merchant",
            //    "nessameowmeow",
            //    "afterofficettv",
            //    "mtg_joe",
            //    "lsv",
            //    "hellogoodgame",
            //    "Abeardedhusky",
            //    "Aliasv",
            //    "Alieldrazi",
            //    "Annamae",
            //    "Calebdmtg",
            //    "Covertgoblue",
            //    "Crokeyz",
            //    "Day9tv",
            //    "Elmagiquero",
            //    "Fffreakmtg",
            //    "Gabyspartz",
            //    "Henip",
            //    "Hueywj",
            //    "Insaynehayne",
            //    "Jeffhoogland",
            //    "Jimdavismtg",
            //    "Legenvd",
            //    "Magicshibby",
            //    "Martinjuza",
            //    "Martypunker",
            //    "Mtgbbd",
            //    "Nessameowmeow",
            //    "Numotthenummy",
            //    "Predimtg",
            //    "Pvddr",
            //    "Reiderrabbit",
            //    "Santosvella",
            //    "Sethmanfieldmtg",
            //    "Shahar_shenhar",
            //    "Shinlak",
            //    "Sjow",
            //    "Slimo0",
            //    "Theasianavenger",
            //    "Thundermo_hellkite",
            //    "Tigerzord",
            //    "Toddstevensmtg",
            //    "Truedawnfm",
            //    "Wyattdarbymtg",
            //};

            var manualAetherhub = new[]
            {
                "user_porkchopsocks",
                "user_arenaunderground",
                "user_aliasv",
                "user_drspilikin",
                "user_covertgoblue",
                "user_mtgjeff",
                "user_sentienslair",
                "user_ratsrelyk",
                "user_totalmtg",
                "user_merchant",
                "user_absurd_heroine",
                "user_jlowry51",
                "user_merlin0012000",
                "user_perplexitytv",
                "user_hellogoodgame",
                "user_waifugate",
                "user_vladislaugh",
                "user_legenvd",
                "user_captainjamoke",
                "user_mtgarenaoriginaldecks",
                "user_eliott_dragon",
                "user_keetsune",
                "user_ravenx",
                "user_powrdragn",
                "user_thaeyn",
                "user_prepcoin",
                "user_supermadlad",
                "user_silentbirds",
                "user_jdoubler2",
                "user_donnerstagsdrafter",
                "user_thyrixsyx",
                "user_riskshocker",
                "user_bandit_mtg",
                "user_adamantmtg",
                "user_lukinhasow",
                "user_fbadaro",
                "user_ramonkcom",
                "user_stephenhornemtg",
                "user_kdog3030",
                "user_timcatn",
                "user_mtg_joe",
                "user_theskartv",
                "user_hamhocks42",
                "user_billyred",
                "user_magicmalgiocato",
                "user_danytlaw",
                "user_alwaysboltthebird",
                "user_fedezic",
                "user_titansfan920",
                "user_vodkacrusader",
                "user_killadub",
                "user_crok",
                "user_Adamantmtg",
                "user_Aliasv",
                "user_Bunnymage",
                "user_Covertgoblue",
                "user_Drspilikin",
                "user_Empty23",
                "user_Fathermapple",
                "user_Fedezic",
                "user_Kroneker",
                "user_Legenvd",
                "user_Lukinhasow",
                "user_Merchant",
                "user_Mogwai",
                "user_Monoblackmagic",
                "user_Mtg_arena_meta",
                "user_Mtg_joe",
                "user_Mtgarenaoriginaldecks",
                "user_Mtgjeff",
                "user_Prepcoin",
                "user_Professornoxlive",
                "user_Sentienslair",
                "user_Slayer007",
                "user_Thyrixsyx",
                "user_Waifugate",
            };

            var ret = ConfigDecks.Get()
                .Select(i => new ScraperType(i.ScraperTypeId))
                .Where(i => i.IsByUser)
                .GroupBy(i => (i.Type, i.Format))
                .ToDictionary(i => i.Key, i => i.Select(x => x.Name).Distinct().ToList());

            // Ensure that each Aetherhub user is present in each format (Standard / Arena)
            var keyAetherhubStandard = (ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.Standard);
            var keyAetherhubArena = (ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.ArenaStandard);
            var keyAetherhubHistoricBo1 = (ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo1);
            var keyAetherhubHistoricBo3 = (ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo3);

            if (ret.ContainsKey(keyAetherhubHistoricBo1) == false)
                ret.Add(keyAetherhubHistoricBo1, new List<string>());
            if (ret.ContainsKey(keyAetherhubHistoricBo3) == false)
                ret.Add(keyAetherhubHistoricBo3, new List<string>());

            ICollection<string> aetherhubUsers = Array.Empty<string>();
            if (ret.ContainsKey(keyAetherhubStandard))
                aetherhubUsers = ret[keyAetherhubStandard];

            if (ret.ContainsKey(keyAetherhubArena))
                aetherhubUsers = aetherhubUsers.Union(ret[keyAetherhubArena])
                .Distinct()
                .ToArray();

            if (ret.ContainsKey(keyAetherhubHistoricBo1))
                aetherhubUsers = aetherhubUsers.Union(ret[keyAetherhubHistoricBo1])
                .Distinct()
                .ToArray();

            if (ret.ContainsKey(keyAetherhubHistoricBo3))
                aetherhubUsers = aetherhubUsers.Union(ret[keyAetherhubHistoricBo3])
                .Distinct()
                .ToArray();

            foreach (var aetherhubUser in aetherhubUsers)
            {
                if (ret.ContainsKey(keyAetherhubStandard) && ret[keyAetherhubStandard].Contains(aetherhubUser) == false)
                    ret[keyAetherhubStandard].Add(aetherhubUser);

                if (ret.ContainsKey(keyAetherhubArena) && ret[keyAetherhubArena].Contains(aetherhubUser) == false)
                    ret[keyAetherhubArena].Add(aetherhubUser);

                if (ret.ContainsKey(keyAetherhubHistoricBo1) && ret[keyAetherhubHistoricBo1].Contains(aetherhubUser) == false)
                    ret[keyAetherhubHistoricBo1].Add(aetherhubUser);

                if (ret.ContainsKey(keyAetherhubHistoricBo3) && ret[keyAetherhubHistoricBo3].Contains(aetherhubUser) == false)
                    ret[keyAetherhubHistoricBo3].Add(aetherhubUser);
            }

            // Manual users to include
            //if (ret.ContainsKey((ScraperTypeEnum.Streamdecker, ScraperTypeFormatEnum.Unknown)) == false)
            //    ret.Add((ScraperTypeEnum.Streamdecker, ScraperTypeFormatEnum.Unknown), new List<string>());
            //foreach (var u in manualStreamdeckers.Where(i => ret[(ScraperTypeEnum.Streamdecker, ScraperTypeFormatEnum.Unknown)].Contains(i) == false))
            //    ret[(ScraperTypeEnum.Streamdecker, ScraperTypeFormatEnum.Unknown)].Add(u);

            if (ret.ContainsKey((ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.ArenaStandard)) == false)
                ret.Add((ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.ArenaStandard), new List<string>());
            foreach (var u in manualAetherhub.Where(i => ret[(ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.ArenaStandard)].Contains(i) == false))
                ret[(ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.ArenaStandard)].Add(u);

            if (ret.ContainsKey((ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.Standard)) == false)
                ret.Add((ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.Standard), new List<string>());
            foreach (var u in manualAetherhub.Where(i => ret[(ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.Standard)].Contains(i) == false))
                ret[(ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.Standard)].Add(u);

            if (ret.ContainsKey((ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo1)) == false)
                ret.Add((ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo1), new List<string>());
            foreach (var u in manualAetherhub.Where(i => ret[(ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo1)].Contains(i) == false))
                ret[(ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo1)].Add(u);

            if (ret.ContainsKey((ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo3)) == false)
                ret.Add((ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo3), new List<string>());
            foreach (var u in manualAetherhub.Where(i => ret[(ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo3)].Contains(i) == false))
                ret[(ScraperTypeEnum.Aetherhub, ScraperTypeFormatEnum.HistoricBo3)].Add(u);

            return ret.ToDictionary(i => i.Key, i => (ICollection<string>)i.Value);
        }

        public async Task SetUserWeights(string userId, IReadOnlyDictionary<RarityEnum, UserWeightDto> weights)
        {
            Log.Information("SetUserWeights({userId}, {weights})", userId, JsonConvert.SerializeObject(weights));

            if (weights == null)
                return;

            await ConfigUsers.MutateUser(userId).PutUserWeights(weights);
        }

        public IReadOnlyDictionary<RarityEnum, UserWeightDto> GetUserWeights(string userId)
        {
            var configUser = ConfigUsers.Get(userId);
            return configUser.Weights.OrDefaultValues();
        }

        public async Task StopNotification(string userId, string notificationId)
        {
            Log.Information("StopNotification({userId}, {notificationId})", userId, notificationId);

            await ConfigUsers.MutateUser(userId).StopNotification(notificationId);
        }

        public async Task ResetNotifications(string userId)
        {
            Log.Information("ResetNotifications({userId})", userId);

            await ConfigUsers.MutateUser(userId).ResetNotifications();
        }

        public void ClearUser(string userId)
        {
            Log.Information("ClearUser({userId})", userId);

            // Unload from memory
            ConfigUsers.Remove(userId);

            //// Delete files
            //var filesToDelete = new[]
            //{
            //    Path.Combine(ConfigApp.FolderDataConfigUsers, $"{userId}_collection.txt"),
            //    Path.Combine(ConfigApp.FolderDataConfigUsers, $"{userId}_userconfig.json"),
            //};

            //foreach (var f in filesToDelete.Where(i => File.Exists(i)))
            //    File.Delete(f);

            var directoryUser = Path.Combine(ConfigApp.FolderDataConfigUsers, userId);
            if (Directory.Exists(directoryUser))
                Directory.Delete(directoryUser, true);
        }

        public DateTime GetVersionUtc()
        {
            var myFiles = Directory.GetFiles(ConfigApp.FolderDlls, "MTGAHelper*.dll", SearchOption.AllDirectories);
            var timestampUtc = myFiles.Select(i => new FileInfo(i).LastWriteTimeUtc).MaxBy(i => i);

            Log.Debug("Hello! Version {timestampUtc}", timestampUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            return timestampUtc;
        }

        public async Task UpdateLandsPreference(string userId, ICollection<int> lands)
        {
            var strLands = JsonConvert.SerializeObject(lands);
            Log.Information("UpdateLandsPreference({userId}, {scrapersActive})", userId, strLands);

            var configUser = ConfigUsers.Get(userId);
            if (configUser == null)
                return;

            await ConfigUsers.MutateUser(userId).UpdateLandPreference(lands);
        }

        private async Task<T> StopwatchOperation<T>(string name, Func<Task<T>> action)
        {
            var sw = Stopwatch.StartNew();

            var ret = await action();

            sw.Stop();
            if (sw.ElapsedMilliseconds >= 1000)
                Log.Information("SessionContainer Stopwatch {name} {time} s", name, (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            return ret;
        }

        private T StopwatchOperation<T>(string name, Func<T> action)
        {
            var sw = Stopwatch.StartNew();

            var ret = action();

            sw.Stop();
            if (sw.ElapsedMilliseconds >= 1000)
                Log.Information("SessionContainer Stopwatch {name} {time} s", name, (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            return ret;
        }
    }
}