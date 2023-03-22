using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using Newtonsoft.Json;

namespace MTGAHelper.Server.Data.Files.UserHistory
{
    public class UserHistoryLoaderFromFile
    {
        readonly FileLoader fileLoader;
        readonly string folderDataConfigUsers;

        public UserHistoryLoaderFromFile(
            FileLoader fileLoader,
            IConfigUsersPath config
            )
        {
            this.fileLoader = fileLoader;
            folderDataConfigUsers = config.FolderDataConfigUsers;
        }

        async Task<string?> LoadDataForDate(string userId, string type, string dateString)
        {
            if (dateString == null || folderDataConfigUsers == null)
                return null;

            var filePath = Path.Combine(folderDataConfigUsers, userId, dateString, $"{userId}_{type}{(string.IsNullOrWhiteSpace(dateString) ? "" : "_" + dateString)}.json");
            return await fileLoader.ReadFileContentAsync(filePath, userId);
        }

        Task<string?> LoadDataForDate(string userId, InfoByDateKeyEnum type, string dateString)
        {
            return LoadDataForDate(userId, type.ToString(), dateString);
        }

        Task<string?> LoadDataAsync(string userId, string type)
        {
            return LoadDataForDate(userId, type, "");
        }

        public async Task SaveToDiskGenericAsync<T>(string userId, string type, T data)
        {
            var dirUserId = Path.Combine(folderDataConfigUsers, userId);
            Directory.CreateDirectory(dirUserId);

            var serialized = JsonConvert.SerializeObject(data);
            //var serialized = System.Text.Json.JsonSerializer.Serialize(data);

            var filePath = Path.Combine(dirUserId, $"{userId}_{type}.json");
            await fileLoader.SaveToDiskAsync(filePath, serialized, userId);
        }

        public async Task SaveToDiskOldAsync<T>(string userId, InfoByDateKeyEnum type, string dateString, InfoByDate<T> data)
        {
            var dirUserIdDate = Path.Combine(folderDataConfigUsers, userId, dateString);
            Directory.CreateDirectory(dirUserIdDate);

            var serialized = JsonConvert.SerializeObject(data);
            //var serialized = System.Text.Json.JsonSerializer.Serialize<InfoByDate<T>>(data);
            //if (type == InfoByDateKeyEnum.PlayerProgress)
            //    Debugger.Break();

            var filePath = Path.Combine(dirUserIdDate, $"{userId}_{type}_{dateString}.json");
            await fileLoader.SaveToDiskAsync(filePath, serialized, userId);
        }

        public async Task SaveToDiskAsync<T>(string userId, InfoByDateKeyEnum type, string dateString, InfoByDate<Dictionary<DateTime, T>> data)
        {
            var dirUserIdDate = Path.Combine(folderDataConfigUsers, userId, dateString);
            Directory.CreateDirectory(dirUserIdDate);

            var serialized = JsonConvert.SerializeObject(data);
            //var serialized = System.Text.Json.JsonSerializer.Serialize(data);

            var filePath = Path.Combine(dirUserIdDate, $"{userId}_{type}_{dateString}.json");
            await fileLoader.SaveToDiskAsync(filePath, serialized, userId);
        }

        public async Task<InfoByDate<T>> GetDataOldAsync<T>(string userId, string dateString, InfoByDateKeyEnum dataType)
        {
            try
            {
                var fileContent = await LoadDataForDate(userId, dataType, dateString);
                if (fileContent != null)
                    return JsonConvert.DeserializeObject<InfoByDate<T>>(fileContent);

                return null;
            }
            catch (Exception ex)
            {
                Debugger.Break();
                throw;
            }
        }

        public async Task<T> GetDataGenericAsync<T>(string userId, string dataType)
        {
            var fileContent = await LoadDataAsync(userId, dataType);
            if (fileContent != null)
                return JsonConvert.DeserializeObject<T>(fileContent);

            return default(T);
        }

        public async Task<InfoByDate<Dictionary<DateTime, T>>> GetDataAsync<T>(string userId, string dateString, InfoByDateKeyEnum dataType)
        {
            var fileContent = await LoadDataForDate(userId, dataType, dateString);
            if (fileContent != null)
                return JsonConvert.DeserializeObject<InfoByDate<Dictionary<DateTime, T>>>(fileContent);

            return null;
        }
    }
}
