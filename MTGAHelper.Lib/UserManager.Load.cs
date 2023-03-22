using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Entity.UserHistory;
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
    public partial class UserManager
    {
        internal async Task<IImmutableUser> LoadUser(string userId, string referer = null)
        {
            ConfigModelUser configUser;
            var configFile = Path.Combine(ConfigPath.FolderDataConfigUsers, userId, $"{userId}_userconfig.json");

            var sw = Stopwatch.StartNew();
            if (File.Exists(configFile) && new FileInfo(configFile).Length > 0)
            {
                //LogExt.LogReadFile(configFile, userId);
                //var fileContent = File.ReadAllText(configFile);
                var fileContent = await fileLoader.ReadFileContentAsync(configFile, userId);

                try
                {
                    configUser = JsonConvert.DeserializeObject<ConfigModelUser>(fileContent);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Log.Warning(
                            "INVALID JSON!!! Trying to remove last char...Loading config from disk for user {userId}",
                            userId);
                        var fileContentRemove1Char = fileContent.Substring(0, fileContent.Length - 1);
                        configUser = JsonConvert.DeserializeObject<ConfigModelUser>(fileContentRemove1Char);
                    }
                    catch (Exception ex2)
                    {
                        Log.Error(
                            "INVALID JSON!!! COULD NOT RECOVER CONFIG from disk for user {userId}, making backup and creating new.",
                            userId);
                        File.Copy(configFile, $"{configFile}.bak{DateTime.Now.ToString("yyyyMMdd_HHmmss")}");
                        configUser = new ConfigModelUser
                        {
                            Id = userId,
                        };
                    }
                }


                try
                {
                    var lastLogin =
                        $"{(DateTime.UtcNow - configUser.LastLoginUtc).TotalHours.ToString("#,0.0")} hours ago";
                    Log.Information(
                        "Loaded config from disk for user {userId} - Last login was {lastLogin}",
                        userId,
                        lastLogin);
                }
                catch (NullReferenceException ex)
                {
                    Log.Error("NULLREF TOFIX: {o1} {o2}", configUser, configUser?.LastLoginUtc);
                }

                foreach (var d in configUser.CustomDecks.Where(i => i.Value.Cards == null))
                {
                    var cardsMain = d.Value.CardsMain.Select(
                        i => new DeckCardRaw
                        {
                            GrpId = i.Key,
                            Amount = i.Value,
                            Zone = DeckCardZoneEnum.Deck
                        });

                    var cardsSideboard = d.Value.CardsSideboard.Select(
                        i => new DeckCardRaw
                        {
                            GrpId = i.Key,
                            Amount = i.Value,
                            Zone = DeckCardZoneEnum.Sideboard,
                        });

                    // Migrate to new model
                    d.Value.Cards = cardsMain.Union(cardsSideboard).ToArray();
                }
            }
            else
            {
                configUser = new ConfigModelUser(userId);
                Log.Information("New user config file created: {userId} - referer:{referer}", userId, referer);
            }
            sw.Stop();
            Log.Debug("SW1 {sw1} s", (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            //sw = Stopwatch.StartNew();
            ////LoadFullHistoryFromDisk(configUser);
            await LoadSummaryHistoryFromDisk(userId, configUser);
            //var latestDate = FindLatestDate(configUser.Id);
            //LoadDate(configUser, latestDate);
            //LoadMtgaDecks(configUser);
            //sw.Stop();
            //Log.Debug("SW_H {sw1} s", (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            //sw = Stopwatch.StartNew();
            LoadUserCustomDecks(configUser);
            //sw.Stop();
            //Log.Debug("SW2 {sw1} s", (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            //sw = Stopwatch.StartNew();
            ConfigUsers.Set(configUser);
            //sw.Stop();
            //Log.Debug("SW3 {sw1} s", (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            return configUser;
        }

        void LoadUserCustomDecks(IImmutableUser configUser)
        {
            //var test = configUser.CustomDecks.First().Value.CardsMain.Where(x => dictAllCards.ContainsKey(x.Key) == false).ToArray();
            // test = configUser.CustomDecks.First().Value.CardsSideboard.Where(x => dictAllCards.ContainsKey(x.Key) == false).ToArray();
            // Load custom decks
            foreach (var d in configUser.CustomDecks.Where(i => i.Value.Cards.All(x => allCards.ContainsKey(x.GrpId))))
            {
                var configDeck = d.Value;

                //try
                //{
                var cards = configDeck.ToDeckCards(allCards);

                d.Value.Deck = new Deck(configDeck.Name, new ScraperType(configDeck.ScraperTypeId), cards);
                //}
                //catch( Exception ex)
                //{
                //    Debugger.Break();
                //}

                //lock (lockDecksUserDefined)
                //{
                configUser.DataInMemory.DecksUserDefined[d.Value.Id] = d.Value.Deck;
                //}
            }
        }

        async Task LoadSummaryHistoryFromDisk(string userId, IImmutableUser configUser)
        {
            if (configUser == null)
            {
                Log.Error("{userId} WEIRD: configUser is null", userId);
            }
            else
            {
                var filepathHistorySummary = GetFileForHistorySummary(configUser.Id);
                if (File.Exists(filepathHistorySummary))
                {
                    //LogExt.LogReadFile(f, configUser.Id);
                    //var content = File.ReadAllText(f);
                    var fileContent = await fileLoader.ReadFileContentAsync(filepathHistorySummary, userId);
                    configUser.DataInMemory.SetHistorySummary(JsonConvert.DeserializeObject<IList<HistorySummaryForDate>>(fileContent));
                }
            }
        }
    }
}
