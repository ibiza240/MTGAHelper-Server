using AutoMapper;
using Serilog;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Minmaxdev.Data.Persistence.File.Service
{
    public class FileManipulator<TModelFile>
    {
        private static JsonSerializerOptions OptionEnumAsString = new JsonSerializerOptions
        {
            WriteIndented = false,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        private readonly ILogger logger;
        private readonly IMapper mapper;
        private readonly object lockFile = new object();

        public FileManipulator(
            ILogger logger,
            IMapper mapper
            )
        {
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task Save(string filepath, TModelFile model)
        {
            if (filepath == default)
                System.Diagnostics.Debugger.Break();

            var fullPath = Path.GetFullPath(filepath);
            var fileContent = JsonSerializer.Serialize(model, OptionEnumAsString);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            var iTry = 0;

            while (true)
            {
                try
                {
                    //if (logSave)
                    logger.Information("{operation} {filePath}", nameof(Save), fullPath);
                    //else
                    //    logger.Debug("{operation} {filePath}", nameof(Save), fullPath);

                    lock (lockFile)
                    {
                        using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
                        using (StreamWriter writer = new StreamWriter(fs))
                        {
                            writer.Write(fileContent);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    iTry++;
                    if (iTry >= 5)
                    {
                        logger.Error(ex, "{operation} {filePath} Failure #{try}>", nameof(Save), fullPath, iTry);
                        throw;
                    }

                    logger.Error("{operation} {filePath} Failure #{try}>", nameof(Save), fullPath, iTry);
                    await Task.Delay(iTry * iTry * 1000);
                }
            }
        }

        public async Task<TModelFile> Load(string filepath, bool logDocumentNotFound)
        {
            //if (filepath == default || filepath.Contains("formats.json"))
            if (filepath == default)
                System.Diagnostics.Debugger.Break();

            var fullPath = Path.GetFullPath(filepath);

            if (System.IO.File.Exists(filepath))
                logger.Information("{operation} {filePath}", nameof(Load), fullPath);
            else
            {
                if (logDocumentNotFound)
                    logger.Error("{operation} {filePath} File not found", nameof(Load), fullPath);
                else
                    logger.Debug("{operation} {filePath} File not found", nameof(Load), fullPath);

                //throw new FileNotFoundException($"{nameof(Load)} {filepath} File not found", fullPath);
                return default;
            }

            int iTry = 0;
            while (true)
            {
                try
                {
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var result = await JsonSerializer.DeserializeAsync<TModelFile>(stream, OptionEnumAsString);
                        if (result == null)
                            logger.Warning("File with no data <{filename}>", fullPath);

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    iTry++;
                    if (iTry >= 5)
                    {
                        logger.Error(ex, "{operation} {filePath} Problem reading file", nameof(Load), fullPath);
                        throw;
                    }

                    // Wait a bit before retrying in case of failure
                    await Task.Delay(500);
                }
            }
        }
    }
}