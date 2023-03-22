using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.OutputLogParser;
using MTGAHelper.Lib.OutputLogParser.Models;

namespace MTGAHelper.Web.UI.Shared
{
    public interface IFileCollectionDeflator
    {
        Task<(OutputLogResult result, OutputLogResult2 result2)> UnzipAndGetCollection(string userId, IFormFile file);
        void SaveFile(string userId, IFormFile file, string directory, string suffix = "");
    }

    public class FileCollectionDeflator : IFileCollectionDeflator
    {
        ConfigModelApp configApp;
        ZipDeflator deflator;

        public FileCollectionDeflator(
            ConfigModelApp configApp,
            ZipDeflator deflator
        )
        {
            this.configApp = configApp;
            this.deflator = deflator;
        }

        public async Task<(OutputLogResult result, OutputLogResult2 result2)> UnzipAndGetCollection(string userId, IFormFile file)
        {
            (OutputLogResult result, Guid? errorId, OutputLogResult2 result2) = await deflator.UnzipAndGetCollection(userId, file.OpenReadStream());

            if (result.CollectionByDate.Any(i => i.DateTime == default(DateTime)))
                SaveFile(userId + "_NODATE_", file, configApp.FolderInvalidZips);

            if (errorId.HasValue)
                SaveFile(userId + "_", file, Path.Combine(configApp.FolderInvalidZips, "parserError"), "_" + errorId.ToString());

            return (result, result2);
        }

        public void SaveFile(string prefix, IFormFile file, string directory, string suffix = "")
        {
            if (file == null)
                return;

            Directory.CreateDirectory(directory);

            using (var fs = new FileStream(Path.Combine(directory, $"{prefix}{file.FileName}{suffix}"), FileMode.Create))
            {
                file.CopyTo(fs);
            }
        }
    }
}
