using System.Collections.Generic;
using System.IO;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.Config;
using Newtonsoft.Json;

namespace MTGAHelper.Lib.CacheLoaders
{
    public class CacheLoaderAccounts : ICacheLoader<IReadOnlyDictionary<string, AccountModel>>
    {
        private readonly string folderAccounts;

        public CacheLoaderAccounts(IAccountPath folderAccounts)
        {
            this.folderAccounts = folderAccounts.FolderDataAccounts;
        }

        public IReadOnlyDictionary<string, AccountModel> LoadData()
        {
            var result = new Dictionary<string, AccountModel>();
            var files = Directory.GetFiles(folderAccounts, "*.json");

            //foreach (var f in files)
            //{
            //    var content = File.ReadAllText(f);
            //    var data = JsonConvert.DeserializeObject<AccountModel>(content);
            //    //if (data.Email == "c_schumacher@hotmail.de") System.Diagnostics.Debugger.Break();
            //    result.Add(data.Email, data);
            //}

            return result;
        }
    }
}