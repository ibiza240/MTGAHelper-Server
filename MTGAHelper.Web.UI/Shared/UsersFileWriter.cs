//using Microsoft.Extensions.Options;
//using MTGAHelper.Lib.Config;
//using Newtonsoft.Json;
//using Serilog;
//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;

//namespace MTGAHelper.Web.UI.Shared
//{
//    public class UsersFileWriter
//    {
//        ConfigModelApp configApp;
//        IConfigManagerUsers configUsers;

//        public UsersFileWriter(IOptionsMonitor<ConfigModelApp> configApp, IConfigManagerUsers configUsers)
//        {
//            this.configApp = configApp.CurrentValue;
//            this.configUsers = configUsers;
//        }

//        public async Task Start(CancellationToken cancellationToken)
//        {
//            await SaveUsersEveryMinute(cancellationToken);
//        }

//        Task SaveUsersEveryMinute(CancellationToken cancellationToken)
//        {
//            Task task = null;

//            // Start a task and return it
//            task = Task.Run(() =>
//            {
//                while (true)
//                {
//                    try
//                    {
//                        Log.Information("Saving users");

//                        var sw = Stopwatch.StartNew();
//                        //File.WriteAllText(Path.Combine(configApp.FolderDataConfigUsers, $"{userId}_userconfig.json"), s);
//                        ////Log.Information("Config {configName} saved for user {userId}!", typeof(IImmutableUser).Name, userId);
//                        var usersData = JsonConvert.SerializeObject(configUsers.Values);
//                        sw.Stop();
//                        Log.Debug("SW7 {sw1} s", (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

//                        sw = Stopwatch.StartNew();
//                        File.WriteAllText(Path.Combine(configApp.FolderData, $"users.json"), usersData);
//                        sw.Stop();
//                        Log.Debug("SW8 {sw1} s", (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

//                        Log.Information("Saved users, count: {usersLength}", usersData.Length);
//                    }
//                    catch (Exception ex)
//                    {
//                        Log.Error(ex, "Unexpected error in thread for DownloaderQueueAsync:");
//                    }

//                    // Save again in 5 minutes
//                    Thread.Sleep(1 * 60 * 1000);
//                }
//            });

//            return task;
//        }
//    }
//}