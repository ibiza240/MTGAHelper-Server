using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Users;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MTGAHelper.Lib.Config.Users
{
    public class ConfigUserCleaner
    {
        private readonly string folderData;

        public ConfigUserCleaner(IDataPath config)
        {
            folderData = config.FolderData;
        }

        public int DeleteConfigFilesBefore(DateTime dateDeleteBefore, string filterPrefix = null)
        {
            var nbDeleted = 0;

            var userDirectories = Directory.GetDirectories(Path.Combine(folderData, "configusers"));
            var iCheck = 0;
            foreach (var directory in userDirectories)
            {
                try
                {
                    var userId = Path.GetFileName(directory);

                    // Ugly way to check this instead of using a .Where(), just to save a double call to Path.GetFileName
                    if (filterPrefix != null && userId.StartsWith(filterPrefix) == false)
                        continue;

                    var file = Path.Combine(directory, $"{userId}_userconfig.json");
                    var content = File.ReadAllText(file);
                    var config = JsonConvert.DeserializeObject<IImmutableUser>(content);
                    var doDelete = false;

                    if (config == null)
                    {
                        // Special case when the config content got wiped out
                        var subDirectories = Directory.GetDirectories(directory).Select(i => Path.GetFileName(i));
                        doDelete = true;
                        var dates = subDirectories
                            .Where(i => DateTime.TryParseExact(i, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                            .Select(i => DateTime.ParseExact(i, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None));

                        if (dates.Any(i => i >= dateDeleteBefore))
                            doDelete = false;
                    }
                    else
                    {
                        // Normal case
                        doDelete = config.NbLogin < 5 && config.LastLoginUtc < dateDeleteBefore;
                    }


                    if (doDelete)
                    {
                        Directory.Delete(directory, true);
                        nbDeleted++;
                    }

                    iCheck++;
                    if (iCheck % 20 == 0)
                        Serilog.Log.Information("ConfigUserCleaner {i} / {total}", iCheck, userDirectories.Length);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    Log.Error("ConfigUserCleaner!!!!! NULL userId for {folder}", directory);
                }
            }

            return nbDeleted;
        }
    }
}
