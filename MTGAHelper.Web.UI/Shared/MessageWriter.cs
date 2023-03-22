using System;
using System.Collections.Generic;
using System.IO;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.Logging;
using Serilog;

namespace MTGAHelper.Web.UI.Shared
{
    public class MessageWriter
    {
        ConfigModelApp configApp;

        string folderMessages => configApp.FolderUserMessages;

        public MessageWriter(ConfigModelApp configApp)
        {
            this.configApp = configApp;
        }

        public void WriteMessage(string userId, string message)
        {
            var suffix = userId;
            if (string.IsNullOrEmpty(suffix))
                suffix = "Unknown" + new Random().Next(1, int.MaxValue);

            var filename = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss-") + suffix + ".txt";
            Directory.CreateDirectory(folderMessages);

            File.WriteAllText(Path.Combine(folderMessages, filename), message);
            Log.Information("Message from user {userId} created: {filename}", userId, filename);
        }

        public ICollection<(string filename, string message)> GetMessages()
        {
            if (Directory.Exists(folderMessages) == false)
                return Array.Empty<(string, string)>();

            var msgs = new List<(string, string)>();
            foreach (var f in Directory.GetFiles(folderMessages))
            {
                LogExt.LogReadFile(f);
                msgs.Add((Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)));
            }

            return msgs;
        }
    }
}
