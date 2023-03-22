using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.IoC;
using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Server.Data.CosmosDB.IoC;
using MTGAHelper.Server.Data.IoC;
using MTGAHelper.Tools.CosmosDB.Downloader.v2;
using SimpleInjector;
using System;
using System.Threading.Tasks;

namespace MTGAHelper.Tools.CosmosDB.Downloader
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var container = CreateContainer();

            Console.WriteLine("Initializing...");
            container.Verify();

            //var folderInput = @"D:\repos-data\MTGAHelper\bak\20210623\data\configusers\";
            var folderOutput = @"D:\repos-data\MTGAHelper\bak\cosmos\";

            var fileMembers = @"D:\repos-data\MTGAHelper\bak\Members.csv";
            var folderAccounts = @"D:\repos-data\MTGAHelper\bak\20220319\data\accounts";
            var supportersUserIds = container.GetInstance<SupportersProvider>().GetSupportersUserIds(fileMembers, folderAccounts);

            var orchestrator = container.GetInstance<Orchestrator>();
            var cosmosManager = await container.GetInstance<UserDataCosmosManager>().Init();
            var cosmosManager2 = await container.GetInstance<UserDataCosmosManager2>().Init(folderOutput);

            /////////////////////////////////////////////////////////////////////////////
            /// NEW WAY

            ////FOR REGULAR MAINTENANCE: DOWNLOAD ALL DATA
            //await container.GetInstance<DownloaderAll>().Download(folderOutput);

            //////FOR REGULAR MAINTENANCE: DELETE OBSELETE DATA
            ////await container.GetInstance<DownloaderAll>().DeleteOlderThan(supportersUserIds, folderOutput, new DateTime(2021, 6, 23));
            // now simpler
            await container.GetInstance<DownloaderAll>().DeleteOlderThan2(supportersUserIds, folderOutput, new DateTime(2021, 9, 15));

            //// FOR UPLOADING DATA TO THE SERVER
            //await new DataUploader(folderOutput, cosmosManager).UploadData("9b74dddfb49242dba9c2409593b1fa19");

            /////////////////////////////////////////////////////////////////////////////
            /// OLD WAY

            ////FOR REGULAR MAINTENANCE: DOWNLOAD ALL DATA AND DELETE OLD DATA USING NORMAL DB
            //await orchestrator.DownloadAllData(supportersUserIds, folderInput, folderOutput);

            ////// FOR UPLOADING DATA TO THE SERVER
            ////new DataUploader(folderOutput, cosmosManager).UploadData("9b74dddfb49242dba9c2409593b1fa19");

            ////// FOR REGULAR MAINTENANCE: DELETE OLD DATA USING NORMAL DB
            ////await orchestrator.CleanOldData(supportersUserIds, folderInput, folderOutput);

            //////////// FOR ADDING A MISSING DATE TO A USER WITH DATA FOR THAT DATE
            //////////var userId = "b03410bdf42e480b81d694e32362191a";
            //////////var date = new DateTime(2020, 9, 13);
            //////////await orchestrator.AddDateToUser(userId, date);

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }

        public static Container CreateContainer()
        {
            var container = new Container()
                .RegisterServicesCosmosDb()
                .RegisterRepositories()
                .RegisterServicesServerData();

            container.Register<Orchestrator>();
            container.Register<SupportersProvider>();
            container.Register<DownloaderAll>();
            container.RegisterSingleton<UserDataCosmosManager2>();
            container.RegisterSingleton<IConfigUsersPath>(() => new ConfigUsersPath());
            return container;
        }

        public class ConfigUsersPath : IConfigUsersPath
        {
            public string FolderDataConfigUsers => null;
        }
    }
}