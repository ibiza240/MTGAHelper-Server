using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Server.Data.Files;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Lib.Config.Users
{
    public class ConfigManagerCustomDraftRatings
    {
        private readonly string folder;

        public ConfigManagerCustomDraftRatings(IConfigUsersPath configPath)
        {
            this.folder = configPath.FolderDataConfigUsers;
        }

        public async Task<ICollection<CustomDraftRating>> Get(string userId)
        {
            var filePath = Path.Combine(folder, userId, $"{userId}_customdraftratings.json");
            var content = await new FileLoader().ReadFileContentAsync(filePath, userId);
            if (content == null)
                return Array.Empty<CustomDraftRating>();

            var data = JsonConvert.DeserializeObject<ICollection<CustomDraftRating>>(content);
            return data;
        }

        public async Task SaveToDisk(string userId, ICollection<CustomDraftRating> ratings)
        {
            // The loop is to try to fix empty files being saved
            var mustSaveFile = true;
            int iTry = 0;
            while (mustSaveFile)
            {
                try
                {
                    var filePath = Path.Combine(folder, userId, $"{userId}_customdraftratings.json");
                    await new FileLoader().SaveToDiskAsync(filePath, JsonConvert.SerializeObject(ratings), userId);

                    await Task.Delay(50);

                    // Confirm that the saved data can be successfully deserialized
                    // Exception will be thrown if the text is invalid JSON
                    var checkSavedData = await File.ReadAllTextAsync(filePath);
                    JsonConvert.DeserializeObject<ICollection<CustomDraftRating>>(checkSavedData);

                    mustSaveFile = false;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while saving user config {userId} to disk", userId);
                    iTry++;
                    mustSaveFile = iTry < 5;
                    await Task.Delay(1000);
                }
            }
        }
    }
}