using AutoMapper;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTGAHelper.Entity;
using MTGAHelper.Entity.CollectionDecksCompare;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.Services;
using MTGAHelper.Lib;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Lib.DraftBoostersCriticalPoint;
using MTGAHelper.Lib.Exceptions;
using MTGAHelper.Lib.MasteryPass;
using MTGAHelper.Lib.MtgaDeckStats;
using MTGAHelper.Lib.Scraping.DeckSources;
using MTGAHelper.Lib.UserHistory;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.Queries;
using MTGAHelper.Web.Models.Request;
using MTGAHelper.Web.Models.Response;
using MTGAHelper.Web.Models.Response.Deck;
using MTGAHelper.Web.Models.Response.User;
using MTGAHelper.Web.Models.Response.User.History;
using MTGAHelper.Web.Models.Response.User.Match;
using MTGAHelper.Web.Models.SharedDto;
using MTGAHelper.Web.UI.Shared;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Web.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : MtgaHelperControllerBase
    {
        private readonly IMapper mapper;
        private readonly IReadOnlyDictionary<int, Card> allCards;
        private readonly ConfigModelApp configApp;
        private readonly IFileCollectionDeflator fileDeflator;
        private readonly IDecksDownloader decksDownloader;

        //UserHistoryParser historyParser;
        private readonly DecksDownloaderQueueAsync downloaderQueue;

        private readonly RawDeckConverter rawDeckConverter;
        private readonly MtgaDeckSummaryBuilder mtgaDeckSummaryBuilder;
        private readonly MtgaDeckDetailBuilder mtgaDeckDetailBuilder;
        private readonly MtgaDeckAnalysisBuilder mtgaDeckAnalysisBuilder;
        private readonly MtgaDeckLimitedResultsBuilder mtgaDeckLimitedResultsBuilder;
        private readonly CollectionExporter collectionExporter;
        private readonly AccountRepository accountRepository;
        private readonly MasteryPassContainer masteryPassContainer;
        private readonly UserHistoryLoader userHistoryLoader;
        private readonly UserHistoryLatestInventoryBuilder userHistoryLatestInventoryBuilder;
        private readonly DraftBoostersCriticalPointCalculator draftBoostersCriticalPointCalculator;
        private readonly Dictionary<int, string> boosterSetByCollationId;
        private readonly IQueryHandler<LatestUserCollectionQuery, InfoByDate<IReadOnlyDictionary<int, int>>> cacheUserHistoryCollection;
        private readonly IQueryHandler<LatestPlayerProgressQuery, InfoByDate<IReadOnlyDictionary<string, PlayerProgress>>> qPlayerProgress;
        private readonly IQueryHandler<LatestRankQuery, IReadOnlyList<ConfigModelRankInfo>> qLatestRank;
        private readonly IQueryHandler<MatchesOnDayQuery, IReadOnlyList<MatchResult>> qMatches;

        private readonly UserCollectionFetcher userCollectionFetcher;

        private readonly PastDraftRetriever pastDraftRetriever;
        private readonly ConfigManagerCustomDraftRatings configManagerCustomDraftRatings;
        private readonly JumpstartCollectionComparer jumpstartCollectionComparer;
        private readonly BasicLandIdentifier basicLandIdentifier;
        private readonly EconomyReportGenerator economyReportGenerator;

        public UserController(
            IMapper mapper,
            ConfigModelApp configApp,
            IConfigManagerUsers configUsers,
            ISessionContainer container,
            IFileCollectionDeflator fileDeflator,
            IDecksDownloader decksDownloader,
            //UserHistoryParser historyParser,
            DecksDownloaderQueueAsync downloaderQueue,
            RawDeckConverter rawDeckConverter,
            MtgaDeckSummaryBuilder mtgaDeckSummaryBuilder,
            MtgaDeckDetailBuilder mtgaDeckDetailBuilder,
            MtgaDeckAnalysisBuilder mtgaDeckAnalysisBuilder,
            MtgaDeckLimitedResultsBuilder mtgaDeckLimitedResultsBuilder,
            CollectionExporter collectionExporter,
            AccountRepository accountRepository,
            MasteryPassContainer masteryPassContainer,
            UserHistoryLoader userHistoryLoader,
            UserHistoryLatestInventoryBuilder userHistoryLatestInventoryBuilder,
            DraftBoostersCriticalPointCalculator draftBoostersCriticalPointCalculator,
            CacheSingleton<IReadOnlyDictionary<int, Set>> cacheSets,
            ICardRepository cardRepo,
            IQueryHandler<LatestUserCollectionQuery, InfoByDate<IReadOnlyDictionary<int, int>>> cacheUserHistoryCollection,
            IQueryHandler<LatestPlayerProgressQuery, InfoByDate<IReadOnlyDictionary<string, PlayerProgress>>> qPlayerProgress,
            IQueryHandler<LatestRankQuery, IReadOnlyList<ConfigModelRankInfo>> qLatestRank,
            IQueryHandler<MatchesOnDayQuery, IReadOnlyList<MatchResult>> qMatches,
            CacheSingleton<IReadOnlySet<string>> cacheMembers,
            UserCollectionFetcher userCollectionFetcher,
            PastDraftRetriever pastDraftRetriever,
            ConfigManagerCustomDraftRatings configManagerCustomDraftRatings,
            JumpstartCollectionComparer jumpstartCollectionComparer,
            BasicLandIdentifier basicLandIdentifier,
            EconomyReportGenerator economyReportGenerator
            )
            : base(cacheMembers, container, configUsers)
        {
            this.configApp = configApp;
            //this.historyParser = historyParser;
            this.fileDeflator = fileDeflator;//.Init(container.allCards);//, container.CardsContainer.mappings);
            this.decksDownloader = decksDownloader.Init(container.GetScraperUsers());
            this.downloaderQueue = downloaderQueue;
            this.rawDeckConverter = rawDeckConverter;
            this.mtgaDeckSummaryBuilder = mtgaDeckSummaryBuilder;
            this.mtgaDeckDetailBuilder = mtgaDeckDetailBuilder;
            this.mtgaDeckAnalysisBuilder = mtgaDeckAnalysisBuilder;
            this.mtgaDeckLimitedResultsBuilder = mtgaDeckLimitedResultsBuilder;
            this.collectionExporter = collectionExporter;
            this.accountRepository = accountRepository;
            this.masteryPassContainer = masteryPassContainer;
            this.userHistoryLoader = userHistoryLoader;
            this.userHistoryLatestInventoryBuilder = userHistoryLatestInventoryBuilder;
            this.draftBoostersCriticalPointCalculator = draftBoostersCriticalPointCalculator;
            //this.cacheUserHistoryCollection = cacheUserHistoryCollection.Init(this.configApp.FolderDataConfigUsers);
            this.cacheUserHistoryCollection = cacheUserHistoryCollection;
            this.userCollectionFetcher = userCollectionFetcher;
            this.pastDraftRetriever = pastDraftRetriever;
            this.configManagerCustomDraftRatings = configManagerCustomDraftRatings;
            this.jumpstartCollectionComparer = jumpstartCollectionComparer;
            this.basicLandIdentifier = basicLandIdentifier;
            this.economyReportGenerator = economyReportGenerator;
            this.mapper = mapper;
            this.qMatches = qMatches;
            this.qLatestRank = qLatestRank;
            this.qPlayerProgress = qPlayerProgress;

            boosterSetByCollationId = cacheSets.Get().ToDictionary(i => i.Key, i => i.Value.Code);
            allCards = cardRepo;
        }

        private async Task<(DateTime dateTime, IReadOnlyCollection<CardWithAmount> cards)> GetLastCollection()
        {
            //var lastCollection = userConfig.DataInMemory.HistoryDetails.GetLastCollection();
            var collectionInfo = await userCollectionFetcher.GetLatestCollection(userId);
            var collection = rawDeckConverter.LoadCollection(collectionInfo.Info);

            //var csvtest = collection
            //    .Where(i => i.Card.set == "M19" && i.Card.rarity.Equals("Common", StringComparison.InvariantCultureIgnoreCase))
            //    .Select(i => $"{i.Card.name}\t{i.Card.number}");

            return (collectionInfo.DateTime, collection);
        }

        private async Task<CollectionResponse> GetResponseLastCollection()
        {
            var userConfig = configUsers.Get(userId);
            (var dateTime, var collection) = await GetLastCollection();
            var inventory = await userHistoryLatestInventoryBuilder.Get(userId);

            //var history = userConfig.DataInMemory.HistoryDetails;
            //var ranks = history.GetLastRankInfo().Info;
            //var progress = history.GetLastProgress().Info;
            var ranks = await qLatestRank.Handle(new LatestRankQuery(userId));
            var progress = (await qPlayerProgress.Handle(new LatestPlayerProgressQuery(userId))).Info;

            var responseBody = new CollectionResponse(userConfig.PlayerName, dateTime, userConfig.LastUploadHash, mapper, collection, inventory, ranks, progress);
            return responseBody;
        }

        // POST api/user/collection
        [HttpPost("collection")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(52428800)]  // 50 MB
        // Do not rename fileCollection! Unless you change the client-side input name also
        public async Task<ActionResult<IResponse>> PostCollection(IFormFile fileCollection)
        {
            var e = await CheckUser();
            if (e != null) return e;

            if (isSupporter == false)
                return Unauthorized();

            try
            {
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                var (result, result2) = await fileDeflator.UnzipAndGetCollection(userId, fileCollection);
                //var persistData = User?.Identity.IsAuthenticated == true;
                await container.SetUserCollection(userId, result);
                var email = User.Identity.GetSubjectId();
                //container.SaveToDatabase(userId, email, result2);
                watch.Stop();

                var info = result.GetLastCollection();
                Log.Information("{userId} Manual PostCollection took {elapsed}s, {collectionDate}, collection:{collectionCount} matches:{matchesCount}",
                    userId, watch.ElapsedMilliseconds / 1000f, info.DateTime, info.Info.Sum(i => i.Value), result.MatchesByDate.Sum(x => x.Info.Count));
            }
            catch (OldDataException ex)
            {
                Log.Warning("User {userId} tried to upload older data: {error}", userId, ex.Message);
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                var msg = $"The collection file is invalid. Possible reasons: 1) You need to go browse your collection in the game because there is no data in the Player.log file. 2) The Player.log file was zipped while the game was running and the zip file got corrupted.";

                if (ex is CardsLoaderException || ex is ParseCollectionBaseException)
                {
                    Log.Warning("User {userId} tried to upload its collection but encountered error: {error}", userId, ex.Message);
                    msg = ex.Message;
                }
                else
                    Log.Error(ex, "User {userId} failed to upload its collection with file {fileName}", userId, fileCollection.FileName);

                fileDeflator.SaveFile(userId, fileCollection, configApp.FolderInvalidZips);
                return BadRequest(new ErrorResponse(msg));
            }

            var responseBody = await GetResponseLastCollection();
            return CreatedAtRoute(nameof(UserController.GetUserCollection), null, responseBody);
        }

        // POST api/user/logfileprocessed
        [HttpPost("logfileprocessed")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        //public ActionResult<IResponse> PostLogFileProcessed([FromBody]PostOutputLogProcessedRequest body)
        public async Task<ActionResult<IResponse>> PostLogFileProcessed()
        {
            var e = await CheckUser();
            if (e != null) return e;

            //Log.Debug(JsonConvert.SerializeObject(body));
            //var test = JsonConvert.SerializeObject(body);
            string bodyStr;
            using (var sr = new StreamReader(Request.Body))
                bodyStr = await sr.ReadToEndAsync();

            var outputLogResult = JsonConvert.DeserializeObject<PostOutputLogProcessedRequest>(bodyStr).OutputLogResult;

            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                //var persistData = User?.Identity.IsAuthenticated == true;
                await container.SetUserCollection(userId, outputLogResult);
            }
            catch (OldDataException ex)
            {
                Log.Warning("Tracker User {userId} tried to upload older data: {error}", userId, ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Tracker User {userId} encountered an error:", userId);
            }
            watch.Stop();

            var info = outputLogResult.GetLastCollection();
            var matchesCountByOutcome = JsonConvert.SerializeObject(outputLogResult.MatchesByDate
                .ToDictionary(i => i.DateTime.ToString("yyyyMMdd"), i => i.Info.GroupBy(x => x.Outcome).ToDictionary(x => x.Key, x => x.Count())));
            Log.Information("{userId} Tracker PostCollection took {elapsed}s, {collectionDate}, collection:{collectionCount} matches:{matchesCountByOutcome}",
                userId, watch.ElapsedMilliseconds / 1000f, info.DateTime, info.Info.Sum(i => i.Value), matchesCountByOutcome);

            var responseBody = await GetResponseLastCollection();
            return CreatedAtRoute(nameof(UserController.GetUserCollection), null, responseBody);
        }

        // POST api/user/logfileprocessed2
        [HttpPost("logfileprocessed2")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> PostLogFileProcessed2()
        {
            var e = await CheckUser();
            if (e != null) return e;

            string bodyStr;
            using (var sr = new StreamReader(Request.Body))
                bodyStr = await sr.ReadToEndAsync();

            try
            {
                var outputLogResult2 = JsonConvert.DeserializeObject<PostOutputLogProcessedRequest2>(bodyStr).OutputLogResult2;
                var email = User.Identity.GetSubjectId();
                container.SaveToDatabase(userId, email, outputLogResult2);
            }
            catch (Exception ex)
            {
                Log.Error("Cannot convert result2 for user {userId}: {msg}", ex.Message, userId);
            }

            return Ok();
        }

        // GET api/user/economy
        [HttpGet("economy", Name = nameof(GetEconomy))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetEconomy()
        {
            var e = await CheckUser();
            if (e != null) return e;

            economyReportGenerator.UserId = userId;
            var responseBody = await StopwatchOperation<EconomyResponse>("GetEconomy", economyReportGenerator.GetResponseEconomy);

            return Ok(responseBody);
        }

        // GET api/user/collection
        [HttpGet("collection", Name = nameof(GetUserCollection))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetUserCollection()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var responseBody = await StopwatchOperation("GetUserCollection", GetResponseLastCollection);

            return Ok(responseBody);
        }

        // GET api/user/collection/missing
        [HttpGet("collection/missing")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetUserCollectionMissing()
        {
            var e = await CheckUser();
            if (e != null) return e;

            //var raw = configUsers.Get(userId).DataInMemory.HistoryDetails.GetLastCollection().Info;
            //var raw = cacheUserHistoryCollection.GetLast(userId).Info;
            var raw = await cacheUserHistoryCollection.Handle(new LatestUserCollectionQuery(userId));

            var collection = StopwatchOperation("GetUserCollectionMissing", () => rawDeckConverter.LoadCollection(raw.Info));

            //var test = collection.Where(i => i.Card.name.StartsWith("Fynn"));

            //var test = container.allCards.Values.Where(i => i.name.Equals("Increasing Vengeance", StringComparison.InvariantCultureIgnoreCase)).ToArray();

            var missingPartial = collection
                .Where(i => i.Amount < 4)
                .Select(i => new CardWithAmount(i.Card, 4 - i.Amount))
                .ToArray();

            //var test4 = missingPartial.Where(i => i.Card.name.Equals("Increasing Vengeance", StringComparison.InvariantCultureIgnoreCase)).ToArray();

            var collectedCards = new HashSet<int>(collection.Select(i => i.Card.GrpId));
            var missingAll = container.AllCards.Values
                .Where(i => i.GrpId != 0)
                .Where(i => collectedCards.Contains(i.GrpId) == false)
                .Select(i => new CardWithAmount(i, 4))
                .ToArray();

            var missingUnion = missingPartial.Concat(missingAll);

            //var test2 = missingUnion.Where(i => i.Card.name.Equals("Increasing Vengeance", StringComparison.InvariantCultureIgnoreCase)).ToArray();

            //var test = missingUnion.Where(i => i.Card.type == null).ToArray();
            //var test = missingUnion.Where(i => i.Card.name.StartsWith("Hallowed"));

            //test = missingUnion.Where(i => i.Card.name.StartsWith("Fynn"));

            var missing = missingUnion
                .Where(i => basicLandIdentifier.IsBasicLand(i.Card) == false)
                .Where(i => i.Card.IsCollectible && i.Card.IsStyle == false)
                .Where(i => i.Card.LinkedFaceType != enumLinkedFace.SplitCard)
                .Where(i => i.Card.LinkedFaceType != enumLinkedFace.DFC_Front)
                .Where(i => i.Card.LinkedFaceType != enumLinkedFace.AdventureAdventure)
                .ToArray();

            //var test3 = missing.Where(i => i.Card.name.Equals("Increasing Vengeance", StringComparison.InvariantCultureIgnoreCase)).ToArray();

            //test = missing.Where(i => i.Card.name.StartsWith("Hallowed"));

            return Ok(new CollectionMissingResponse(mapper.Map<ICollection<CollectionCardDto>>(missing)));
        }

        // GET api/user/register
        [HttpGet("register")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IResponse>> RegisterUser()
        {
            await StopwatchOperation("RegisterUser", async () =>
            {
                var result = await HttpContext.AuthenticateAsync();
                if (result?.Succeeded == true && User?.Identity.IsAuthenticated != true)
                {
                    await HttpContext.SignInAsync(result.Principal);
                }
            });

            return Ok(await RegisterBase());
        }

        //// GET api/user/fromuserid
        //[HttpGet("fromuserid")]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(400)]
        //public ActionResult<IResponse> FromUserId([FromQuery]string id)
        //{
        //    if (string.IsNullOrWhiteSpace(id))
        //    {
        //        Log.Warning("LoginFromUserId {userId}: Please provide a user id", id);
        //        return BadRequest(new ErrorResponse($"Please provide a user id"));
        //    }
        //    else if (container.GetUserStatus(id) == UserStatusEnum.NonExistent)
        //    {
        //        Log.Warning("LoginFromUserId {userId}: This user id is invalid", id);
        //        return BadRequest(new ErrorResponse($"This user id is invalid"));
        //    }

        //    Response.Cookies.Delete("userId");
        //    Response.Cookies.Append("userId", id, new CookieOptions { Expires = DateTime.UtcNow.AddYears(2) });

        //    Log.Information("LoginFromUserId {userId}: success", id);

        //    return Ok(new StatusResponse($"Cookie userId set to {id}"));
        //}
        // GET api/user/changeAccountUserId
        [HttpGet("changeAccountUserId")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult<IResponse> ChangeAccountUserId([FromQuery] string userId)
        {
            if (User?.Identity.IsAuthenticated != true)
            {
                Log.Warning("ChangeAccountUserId {userId}: You must sign-in first", userId);
                return BadRequest(new ErrorResponse($"You must sign-in first"));
            }
            else if (string.IsNullOrWhiteSpace(userId))
            {
                Log.Warning("ChangeAccountUserId {userId}: Please provide a user id", userId);
                return BadRequest(new ErrorResponse($"Please provide a user id"));
            }
            else if (container.GetUserStatus(userId) == UserStatusEnum.NonExistent)
            {
                Log.Warning("ChangeAccountUserId {userId}: This user id is invalid", userId);
                return BadRequest(new ErrorResponse($"This user id is invalid"));
            }

            Response.Cookies.Delete("userId");
            Response.Cookies.Append("userId", userId, new CookieOptions { Expires = DateTime.UtcNow.AddYears(2) });

            var email = User.Identity.GetSubjectId();
            accountRepository.UpdateUserId(email, userId);

            Log.Information("ChangeAccountUserId {userId}: success", userId);

            return Ok(new StatusResponse($"Account UserId set to {userId}"));
        }

        // GET api/user/decks
        [HttpGet("decks")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> Get([FromQuery] string card)
        {
            var e = await CheckUser();
            if (e != null) return e;

            var (totalDecks, decks) = await StopwatchOperation("GetDecks", () => container.GetDecksTracked(userId, new FilterDeckParams { Card = card }));

            var decksNoWeight = decks.Where(i => i.WildcardsMissingMain.Count == 0).ToArray();
            if (decksNoWeight.Any())
                Log.Warning("{nbDecks} decks glitched for {userId}:{list}", decksNoWeight.Length, userId, "\r\n" + string.Join(", ", decksNoWeight.Select(i => i.Name)));

            var decksDto = mapper.Map<ICollection<DeckTrackedSummaryResponseDto>>(decks);

            var test = decksDto.Where(i => i.Name == "Artifact Bo1").ToArray();

            var responseBody = new DeckListResponse<DeckTrackedSummaryResponseDto>(totalDecks, decksDto);
            return Ok(responseBody);
        }

        // POST api/user/preference
        // Saves ONE User preference
        [HttpPost("preference")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> PostUserPreference([FromQuery] string key, [FromQuery] string value)
        {
            var e = await CheckUser();
            if (e != null) return e;

            if (Enum.TryParse(key, out UserPreferenceEnum preferenceEnum))
            {
                await container.SaveUserPreference(userId, preferenceEnum, value);
            }
            else
                Log.Error("{userId} PostUserPreference Invalid preferenceKey: {preferenceKey}", userId, key);

            return Ok(new StatusOkResponse());
        }

        // GET api/user/preferences
        // Gets ALL User preferences
        [HttpGet("preferences")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetUserPreference()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var userPreferences = container.GetUserPreferences(userId);

            return Ok(new UserPreferencesResponse(userPreferences));
        }

        // POST api/user/stopnotification
        [HttpPost("stopnotification")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> StopNotification([FromQuery] string notificationId)
        {
            var e = await CheckUser();
            if (e != null) return e;

            await container.StopNotification(userId, notificationId);

            return Ok(new StatusOkResponse());
        }

        // POST api/user/resetnotifications
        [HttpPost("resetnotifications")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> ResetNotifications()
        {
            var e = await CheckUser();
            if (e != null) return e;

            await container.ResetNotifications(userId);

            return Ok(new StatusOkResponse());
        }

        // PATCH api/user/deckpriorityfactor
        [HttpPatch("deckpriorityfactor")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> PatchDeckPriorityFactor([FromBody] PatchDeckPriorityFactorRequest body)
        {
            var e = await CheckUser();
            if (e != null) return e;

            if ((await container.GetDecksIncludingUserCustom(userId)).ContainsKey(body.DeckId) == false)
            {
                Log.Warning("PatchDeckPriorityFactor, Deck {deckId} not found)", body.DeckId);
                return BadRequest(new ErrorResponse($"Deck {body.DeckId} not found."));
            }

            Log.Information("PatchDeckPriorityFactor: {userId}, {deckId}, {value}, {saveConfig})", userId, body.DeckId, body.Value, true);
            await container.SetDeckPriorityFactor(userId, body.DeckId, body.Value, true);
            //container.Compare(userId);

            return Ok(new StatusOkResponse());
        }

        // PATCH api/user/deckpriorityfactor/resetall
        [HttpPatch("deckpriorityfactor/resetall")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> PatchDeckPriorityFactor_ResetAll([FromBody] PatchDeckPriorityFactorResetAllRequest body)
        {
            var e = await CheckUser();
            if (e != null) return e;

            await container.DeckPriorityFactorResetAll(userId, body.Value);

            return Ok(new StatusOkResponse());
        }

        // PATCH api/user/deckpriorityfactor/filtereddecks
        [HttpPatch("deckpriorityfactor/filtereddecks")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> PatchDeckPriorityFactor_FilteredDecks([FromBody] PatchTrackedDecksRequest body)
        {
            var e = await CheckUser();
            if (e != null) return e;

            await container.SetDeckPriorityFactorForFilteredDecks(userId, body.DoTrack, body.Decks);

            return Ok(new StatusOkResponse());
        }

        // GET api/user/customdecks
        [HttpGet("customdecks")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetCustomDecks()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var decks = configUsers.Get(userId).CustomDecks.Select(i => i.Value).ToArray();

            return Ok(new UserCustomDecksResponse(decks));
        }

        // POST api/user/customdecks
        [HttpPost("customdecks")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> AddCustomDeck([FromBody] PostUserCustomDeckRequest body)
        {
            var e = await CheckUser();
            if (e != null) return e;

            if (string.IsNullOrWhiteSpace(body.Name))
                return BadRequest(new ErrorResponse("The name is required."));

            try
            {
                var deckId = await container.AddCustomDeck(userId, body.Name, body.Url, body.MtgaFormat);
                return Ok(new StatusResponse(deckId));
            }
            catch (InvalidDeckFormatException ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                Log.Warning("User {userId} tried to import custom deck with an invalid format: {message}", userId, message);
                return BadRequest(new ErrorResponse("The provided text is not a valid Arena deck. " + message));
            }
        }

        // DELETE api/user/customdecks/{id}
        [HttpDelete("customdecks/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> DeleteCustomDeck([FromRoute] string id)
        {
            var e = await CheckUser();
            if (e != null) return e;

            if (await container.DeleteCustomDeck(userId, id, true) == false)
                return NotFound(new ErrorResponse($"Deck {id} not found."));

            return Ok(new StatusResponse(id));
        }

        // GET api/user/scrapers
        [HttpGet("scrapers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetScrapers()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var scrapers = await container.GetScrapersInfo(userId);

            return Ok(new UserCustomScraperListResponse(scrapers));
        }

        // POST api/user/scrapers
        [HttpPost("scrapers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> AddScraper([FromBody] PostUserScraperRequest body)
        {
            var e = await CheckUser();
            if (e != null) return e;

            var scraperType = new ScraperType(body.Id.Trim());
            if (scraperType.Name == null)
                return BadRequest(new ErrorResponse("The name is required."));

            // Brought from Lib assembly, to refactor
            async Task<DeckScraperResult> AddScraper(string userId, ScraperType scraperType, IDecksDownloader downloader)
            {
                Log.Information("AddScraper({userId}, {scraperTypeId})", userId, scraperType.Id);

                // Download decks
                downloader.AddDownloader(scraperType);
                var res = downloader.DownloadDecks(scraperType);
                //if (res.Decks.Count == 0)
                //{
                //    Log.Information("User {userId} added custom scraper {scraperTypeId} but it was discarded (0 results)", userId, scraperType.Id);
                //    return new DeckScraperResult { Errors = new List<string> { $"The request for {scraperType.Id} returned 0 results" } };
                //}
                ////else if (res.Errors.Count > 0)
                ////{
                ////    System.Diagnostics.Debugger.Break();
                ////}

                //// Save config deck
                //AddDecks(res.Decks);

                // Save config user
                var configUser = configUsers.Get(userId);

                //if (config.ScrapersActive.Any(i => i.Type == type && i.Name == name) == false)
                //    config.ScrapersActive.Add(new ScraperDto(type, name, res.Decks.Count));
                if (configUser.ScrapersActive.Contains(scraperType.Id))
                    Log.Warning("User {userId} added scraper {scraperType} but it was already there in his config", userId, scraperType.Id);
                else
                {
                    await configUsers.MutateUser(userId).AddScraperActive(scraperType.Id);
                }

                //ConfigDeckUserScrapers.Save();

                ////Compare(userId);

                Log.Information("User {userId} added custom scraper {scraperType} successfully", userId, scraperType.Id);
                return res;
            }

            string[] statusToWatch;
            if (scraperType.Type == ScraperTypeEnum.Aetherhub)
            {
                var s1 = new ScraperType(scraperType.Type, scraperType.Name, ScraperTypeFormatEnum.Standard);
                var s2 = new ScraperType(scraperType.Type, scraperType.Name, ScraperTypeFormatEnum.ArenaStandard);
                await AddScraper(userId, s1, decksDownloader);
                await AddScraper(userId, s2, decksDownloader);

                statusToWatch = new[] { s1.Id, s2.Id };
            }
            else
            {
                var status = await AddScraper(userId, scraperType, decksDownloader);
                //if (status.Errors.Count > 0)
                //    return BadRequest(new ErrorResponse(string.Join(Environment.NewLine, status.Errors)));

                statusToWatch = new[] { body.Id };
            }

            while (downloaderQueue.IdsInQueue.Any(i => statusToWatch.Contains(i)))
                await Task.Delay(1000);

            return Ok(new UserCustomScraperResponse(0, 0));
        }

        // PUT api/user/scrapers
        [HttpPut("scrapers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> UpdateScrapersActive([FromBody] PutUserScraperRequest body)
        {
            var e = await CheckUser();
            if (e != null) return e;

            await container.UpdateScrapersActive(userId, body.ScrapersActive);

            return Ok(new StatusOkResponse());
        }

        // PATCH api/user/scrapers/{id}
        [HttpPatch("scrapers/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> ActivateScraper([FromRoute] string id, [FromBody] PatchScraperActivatedRequest body)
        {
            var e = await CheckUser();
            if (e != null) return e;

            container.ActivateScraper(userId, id, body.Activate);

            return Ok(new StatusOkResponse());
        }

        //// DELETE api/user/scrapers/{id}
        //[HttpDelete("scrapers/{id}")]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        //public ActionResult<IResponse> DeleteCustomScraper([FromRoute]string id)
        //{
        //    var e = CheckUser();
        //    if (e != null) return e;

        //    var scraperType = new ScraperType(id);

        //    if (container.DeleteCustomScraper(userId, scraperType) == false)
        //        return NotFound(new ErrorResponse($"Scraper '{scraperType.Id}' not found."));

        //    return Ok(new StatusOkResponse());
        //}

        // GET api/user/weights
        [HttpGet("weights")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetUserWeights()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var weights = container.GetUserWeights(userId);

            var responseBody = new GetUserWeightsResponse(weights);
            return Ok(responseBody);
        }

        // PUT api/user/weights
        [HttpPut("weights")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> SetUserWeights([FromBody] PutUserWeightsRequest body)
        {
            var e = await CheckUser();
            if (e != null) return e;

            var weights = CardRequiredInfo.DEFAULT_WEIGHTS;
            if (body.Weights != null)
            {
                weights = body.Weights.ToDictionary(i => Enum.Parse<RarityEnum>(i.Key), i => i.Value);
            }

            await container.SetUserWeights(userId, weights);

            return Ok(new StatusOkResponse());
        }

        [HttpGet("history")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetUserHistorySummary([FromQuery] int currentPage, [FromQuery] int perPage)
        {
            var e = await CheckUser();
            if (e != null) return e;

            var configUser = configUsers.Get(userId);
            var historySummary = configUser.DataInMemory.GetHistorySummary().OrderByDescending(i => i.Date).ToArray();

            var (historySummary2, totalItems, datesAvailable) = await userHistoryLoader.LoadSummary(configUser.Id, currentPage, perPage);

            var summary = mapper.Map<GetUserHistorySummaryDto[]>(historySummary);
            var summary2 = mapper.Map<GetUserHistorySummaryDto[]>(historySummary2);

            return Ok(new GetUserHistorySummaryResponse(summary, summary2, totalItems, datesAvailable));
        }

        [HttpGet("history/{date}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetUserHistoryForDate([FromRoute] string date)
        {
            var e = await CheckUser();
            if (e != null) return e;

            var isDateValid = DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateFor);
            if (isDateValid == false)
                return BadRequest(new ErrorResponse($"Invalid date: {date}"));

            var matches = mapper.Map<ICollection<MatchDto>>(await qMatches.Handle(new MatchesOnDayQuery(userId, dateFor)));
            var economyEvents = mapper.Map<ICollection<EconomyEventDto>>(await userHistoryLoader.LoadEconomyEventsForDate(userId, dateFor));
            //var rankUpdated = mapper.Map<ICollection<RankDeltaDto>>(await userHistoryLoader.LoadRankUpdatesForDate(userId, dateFor));
            var rankUpdated = Array.Empty<RankDeltaDto>();

            var history = new GetUserHistoryForDateResponseData(dateFor, matches, economyEvents, rankUpdated);

            new AssociateRankHelper().AssociateRankWithMatches(history);

            return Ok(new GetUserHistoryForDateResponse(history));
        }

        [HttpGet("matchDetails")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetMatchDetails([FromQuery] string matchId, [FromQuery] string matchDate)
        {
            var e = await CheckUser();
            if (e != null) return e;

            if (!DateTime.TryParseExact(matchDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var onDate))
                return BadRequest(new ErrorResponse($"Invalid date: {matchDate}"));

            var configUser = configUsers.Get(userId);
            //userManager.LoadHistoryForDateFromDisk(configUser, date);
            //var historyForDate = configUsers.Get(userId).DataInMemory.HistoryDetails.GetForDate(date);
            //var match = historyForDate.Matches.FirstOrDefault(i => i.MatchId == matchId);
            var match = (await qMatches.Handle(new MatchesOnDayQuery(configUser.Id, onDate)))
                .FirstOrDefault(m => m.MatchId == matchId);

            return Ok(new MatchDetailsResponse(mapper.Map<MatchDto>(match)));
        }

        // PUT api/user/lands
        [HttpPut("lands")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> PutLandsPreference([FromBody] PutUserLandsRequest body)
        {
            var e = await CheckUser();
            if (e != null) return e;

            await container.UpdateLandsPreference(userId, body.Lands);

            return Ok(new StatusOkResponse());
        }

        [HttpGet("lastuploadhash")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetLastUploadHash()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var lastUploadHash = configUsers.Get(userId).LastUploadHash;
            return Ok(new LastHashResponse(lastUploadHash));
        }

        [HttpGet("customDraftRatingsForDisplay")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetCustomDraftRatingsForDisplay()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var ratings = (await configManagerCustomDraftRatings.Get(userId))
                .ToDictionary(i => (i.Set, i.Name), i => i);

            var data = allCards.Values
                .Where(i => i.IsToken == false && i.IsCollectible)
                .OrderBy(i => int.TryParse(i.Number, out int nb) ? nb : 0)
                .Select(i =>
                    new CustomDraftRatingResponseDto
                    {
                        Card = mapper.Map<CardDto>(i),
                        Note = ratings.ContainsKey((i.Set, i.Name)) ? ratings[(i.Set, i.Name)].Note : null,
                        Rating = ratings.ContainsKey((i.Set, i.Name)) ? (int?)ratings[(i.Set, i.Name)].Rating : null,
                    }).ToArray();

            return Ok(data);
        }

        [HttpGet("customDraftRatings")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetCustomDraftRatings()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var ratings = await configManagerCustomDraftRatings.Get(userId);

            var data = ratings
                .Select(i =>
                    new CustomDraftRatingResponseDto
                    {
                        Card = new CardDto { Set = i.Set, Name = i.Name },
                        Note = i.Note,
                        Rating = i.Rating,
                    }).ToArray();

            return Ok(data);
        }

        [HttpPut("customDraftRating")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> PutCustomDraftRating([FromBody] PutUserCustomDraftRatingDto ratingInfo)
        {
            var e = await CheckUser();
            if (e != null) return e;

            // Slow and bad to load them each time
            var ratings = (await configManagerCustomDraftRatings.Get(userId))
                .ToDictionary(i => (i.Set, i.Name), i => i);

            var card = ratingInfo.IdArena == default ?
                new Card(new Card2 { Name = ratingInfo.Name, SetScryfall = ratingInfo.Set }) :
                allCards[ratingInfo.IdArena];

            ratings[(card.Set, card.Name)] = new CustomDraftRating
            {
                Set = card.Set,
                Name = card.Name,
                Rating = ratingInfo.Rating,
                Note = ratingInfo.Note?.Substring(0, Math.Min(4000, ratingInfo.Note.Length)) ?? ""
            };

            await configManagerCustomDraftRatings.SaveToDisk(userId, ratings.Values.ToArray());

            return Ok();
        }

        [HttpGet("compare")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> CompareCollectionWithTrackedDecks()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var data = container.Compare(userId);

            return Ok(new DraftRaredraftingInfoResponse(mapper.Map<ICollection<CardCompareInfo>>(data.ByCard.Values)));
        }

        [HttpGet("mtgadecksummary")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetMtgaDeckSummary([FromQuery] string period)
        {
            var e = await CheckUser();
            if (e != null) return e;

            Log.Information("{userId} GetMtgaDeckSummary", userId);

            var data = await mtgaDeckSummaryBuilder.GetSummary(userId, period);
            return Ok(new GetMtgaDeckSummaryResponse(mapper.Map<ICollection<MtgaDeckSummaryDto>>(data)));
        }

        [HttpGet("mtgadeckdetail")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetMtgaDeckDetail([FromQuery] string deckId, [FromQuery] string period)
        {
            var e = await CheckUser();
            if (e != null) return e;

            Log.Information("{userId} GetMtgaDeckDetail", userId);

            var data = await mtgaDeckDetailBuilder.GetDetail(userId, deckId, period);
            return Ok(new GetMtgaDeckDetailResponse(mapper.Map<MtgaDeckDetailDto>(data)));
        }

        [HttpGet("mtgadeckanalysis")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetMtgaDeckAnalysis(string deckId)
        {
            var e = await CheckUser();
            if (e != null) return e;

            Log.Information("{userId} GetMtgaDeckAnalysis", userId);

            var data = await mtgaDeckAnalysisBuilder.GetDetail(userId, deckId);
            return Ok(new GetMtgaDeckAnalysisResponse(mapper.Map<MtgaDeckAnalysisDto>(data)));
        }

        [HttpGet("statsLimited")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetUserStatsLimited([FromQuery] string period)
        {
            var e = await CheckUser();
            if (e != null) return e;

            Log.Information("{userId} GetUserStatsLimited", userId);

            var data = await mtgaDeckLimitedResultsBuilder.GetLimitedResults(userId, period);
            var payload = mapper.Map<Dictionary<string, UserStatsLimitedResponse>>(data);
            return Ok(payload);
        }

        [HttpGet("exportcollection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> ExportCollection([FromQuery] string format, [FromQuery] bool header, [FromQuery] bool includeNotOwned)
        {
            var e = await CheckUser();
            if (e != null) return e;

            Log.Information("{userId} ExportCollection", userId);

            var valid = collectionExporter.ValidateFormat(format);
            if (valid == false)
                return BadRequest(new ErrorResponse("Invalid parts found"));

            var collection = (await GetLastCollection()).cards.Where(i => i.Card.IsCollectible).ToArray();

            if (includeNotOwned)
            {
                var notOwned = container.AllCards.Values
                    .Where(i => collection.Any(x => x.Card.GrpId == i.GrpId) == false)
                    .Select(i => new CardWithAmount(i, 0))
                    .ToArray();
                collection = collection.Concat(notOwned)
                    .OrderBy(i => i.Card.Name)
                    .ToArray();
            }

            var bytes = collectionExporter.Export(collection, header);

            //if (bytes.Length == 0)
            //    return NotFound();

            // returns a FileStreamResult
            return File(bytes, "application/octet-stream", "collection.txt");
        }

        [HttpGet("draftboosterscriticalpoint")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetDraftBoostersCriticalPoint(
            [FromQuery] string set,
            [FromQuery] float raresPerDraft,
            [FromQuery] float mythicsPerDraft,
            [FromQuery] float packsPerDraft,
            [FromQuery] int additionalPacks)
        {
            var e = await CheckUser();
            if (e != null) return e;

            var setUpper = set.ToUpper();

            var userEmail = User?.Identity?.Name;
            if (userEmail != null && setUpper == configApp.CurrentSet && IsSupporter(userEmail) == false)
                return Unauthorized(new ErrorResponse("Please consider becoming a supporter to get access to the the Draft vs Boosters tool for the latest set and help us maintaining this service :)"));

            var collection = (await GetLastCollection()).cards;
            var inventory = await userHistoryLatestInventoryBuilder.Get(userId);

            var cards = collection
                .Where(i => i.Card.Set == setUpper)
                .Where(i => i.Card.IsFoundInBooster)
                .GroupBy(i => i.Card.Rarity);
            var collectedForSetByRarity = cards
                .ToDictionary(i => i.Key, i => i.Sum(x => x.Amount));

            var cards2 = allCards.Values
                .Where(i => i.Set == setUpper)
                .Where(i => i.IsFoundInBooster)
                .GroupBy(i => i.Rarity);

            var uniqueCountsForSetByRarity = cards2
                .ToDictionary(i => i.Key, i => i.Count());

            var dateFrom = DateTime.UtcNow.Date.AddDays(-14);
            var pastDrafts = await pastDraftRetriever.GetCardsPickedByDraft(userId, dateFrom, set);
            var cardsCollected = pastDrafts
                .Select(i => new
                {
                    rares = i.CardsPicked.Where(c => c.Rarity == RarityEnum.Rare),
                    mythics = i.CardsPicked.Where(c => c.Rarity == RarityEnum.Mythic),
                });
            var avgRares = 0d;
            var avgMythics = 0d;
            if (cardsCollected.Any())
            {
                avgRares = cardsCollected.Average(i => i.rares.Count());
                avgMythics = cardsCollected.Average(i => i.mythics.Count());
            }

            var playerInput = new DraftBoostersCriticalPointPlayerInput
            {
                NbMythics = collectedForSetByRarity.ContainsKey(RarityEnum.Mythic) ? collectedForSetByRarity[RarityEnum.Mythic] : 0,
                NbRares = collectedForSetByRarity.ContainsKey(RarityEnum.Rare) ? collectedForSetByRarity[RarityEnum.Rare] : 0,
                NbPacks = additionalPacks + (inventory.Boosters.FirstOrDefault(i => boosterSetByCollationId[i.CollationId] == setUpper)?.Count ?? 0),
                WcTrackPosition = inventory.WcTrackPosition,
            };

            var assumptions = new DraftBoostersCriticalPointAssumptions
            {
                NbMythicsPerDraft = mythicsPerDraft,
                NbRaresPerDraft = raresPerDraft,
                NbRewardPacksPerDraft = packsPerDraft,
                NbUniqueMythicsInSet = uniqueCountsForSetByRarity[RarityEnum.Mythic],
                NbUniqueRaresInSet = uniqueCountsForSetByRarity[RarityEnum.Rare],
            };

            var result = draftBoostersCriticalPointCalculator.Calculate(playerInput, assumptions);
            return Ok(new DraftBoostersCriticalPointResultDto(result, avgRares, avgMythics));
        }

        [HttpGet("masterypass")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> MasteryPassFinalLevel([FromQuery] int dailyWins = -1, int weeklyWins = -1)
        {
            var e = await CheckUser();
            if (e != null) return e;

            IImmutableUser user;
            if (dailyWins < 0 && weeklyWins < 0)
                user = configUsers.Get(userId);
            else
                user = await configUsers.MutateUser(userId).SetNbExpectedWins(dailyWins, weeklyWins);

            var masteryPassCalculator = await masteryPassContainer.Calculate(user.Id, configApp.CurrentSet, user.NbDailyWinsExpected, user.NbWeeklyWinsExpected);

            var response = mapper.Map<UserMasteryPassResponse>(masteryPassCalculator);
            return Ok(response);
        }

        [HttpGet("issupporter")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<IResponse> CheckIsSupporter(string userEmail)
        {
            var isSupporter = base.IsSupporter(userEmail);
            return Ok(isSupporter);
        }

        // GET api/user/jumpstartmissing
        [HttpGet("jumpstartmissing")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetMissingCardsFromJumpstartThemes([FromQuery] bool onlyStandard, [FromQuery] JumpstartLandWeightingEnum landWeighting)
        {
            var e = await CheckUser();
            if (e != null) return e;

            var collection = (await userCollectionFetcher.GetLatestCollection(userId)).Info;
            var data = jumpstartCollectionComparer.CalculateMissingCardsForThemes(collection, onlyStandard, landWeighting);

            return Ok(data);
        }
    }
}