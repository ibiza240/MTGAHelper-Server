using MTGAHelper.Server.Data.Files;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.JsonFixing
{
    public class MassJsonFileFixer
    {
        FileLoader fileLoader;
        JsonValidator jsonValidator;

        public MassJsonFileFixer(
            FileLoader fileLoader,
            JsonValidator jsonValidator
            )
        {
            this.fileLoader = fileLoader;
            this.jsonValidator = jsonValidator;
        }

        public async Task FixAllAsync(ICollection<string> filepaths)
        {
            foreach (var filepath in filepaths)
            {
                var content = await fileLoader.ReadFileContentAsync(filepath, log: false);

                if (jsonValidator.IsValid(content) == false)
                {
                    var (success, newJson) = jsonValidator.TryToFix(content);
                    if (success)
                    {
                        var newFolder = new System.IO.FileInfo(filepath).Directory.FullName.Replace("data", "datafixed");
                        System.IO.Directory.CreateDirectory(newFolder);
                        var newFilepath = filepath.Replace("data", "datafixed");

                        await fileLoader.SaveToDiskAsync(newFilepath, newJson);
                        Log.Information("File {filepath} contained invalid JSON but has been fixed", filepath);
                    }
                    else
                    {
                        Log.Information("File {filepath} contains invalid JSON and could not be fixed", filepath);
                    }
                }
            }
        }
    }
}
