using System.Collections.Generic;
using System.IO;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Decks;
using Newtonsoft.Json;

namespace MTGAHelper.Lib.Config
{
    public abstract class ConfigManagerBase<T> where T : IConfigModel
    {
        protected abstract string configFileName { get; }
        protected object lockData = new object();
        protected readonly IDataPath configApp;
        protected Dictionary<string, T> dictValues = new Dictionary<string, T>();
        protected abstract dynamic GetRoot();

        public ICollection<T> Values { get { lock (lockData) return dictValues.Values; } }

        public ConfigManagerBase(IDataPath configApp)
        {
            this.configApp = configApp;
        }

        public T Get(string id)
        {
            lock (lockData)
            {
                if (dictValues.ContainsKey(id) == false)
                    return default(T);

                return dictValues[id];
            }
        }

        public void Set(T config)
        {
            lock (lockData)
                dictValues[config.Id] = config;
        }

        public void Remove(string id)
        {
            lock (lockData)
                if (dictValues.ContainsKey(id))
                    dictValues.Remove(id);
        }

        public void Save()
        {
            lock (lockData)
            {
                var s = JsonConvert.SerializeObject(GetRoot());
                File.WriteAllText(Path.Combine(configApp.FolderData, configFileName), s);
            }
        }
    }
}
