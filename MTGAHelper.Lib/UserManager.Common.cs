using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Server.Data.Files;
using MTGAHelper.Server.DataAccess;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.IO;

namespace MTGAHelper.Lib
{
    public partial class UserManager
    {
        //MatchesCacheManager matchesCacheManager;
        private readonly IConfigUsersPath ConfigPath;

        private readonly IConfigManagerUsers ConfigUsers;
        private readonly IReadOnlyDictionary<int, Card> allCards;

        //UserHistoryParser historyParser;
        //UserDataCosmosManager userDataCosmosManager;
        private readonly FileLoader fileLoader;

        private readonly ILogResultPersister logResultPersister;

        public UserManager(
            IConfigUsersPath configPath,
            IConfigManagerUsers configUsers,
            FileLoader fileLoader,
            ICardRepository cardRepo,
            ILogResultPersister logResultPersister)
        {
            this.ConfigPath = configPath;
            this.ConfigUsers = configUsers;
            this.allCards = cardRepo;
            this.fileLoader = fileLoader;
            this.logResultPersister = logResultPersister;
        }

        //public void Init(ICollection<Card> allCards)
        //{
        //    this.allCards = allCards;
        //}

        private string GetFileForHistorySummary(string userId) => Path.Combine(ConfigPath.FolderDataConfigUsers, userId, $"{userId}_history.json");
    }
}