using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Server.Data.Files
{
    public class FileLoader
    {
        public async Task<string> ReadFileContentAsync(string filePath, string userId = "null", bool log = true)
        {
            //if (filePath.Contains("atches") && filePath.Contains("201910")) Debugger.Break(); 

            //var callerMethod = new StackFrame(1, true).GetMethod();
            //var context = $"{callerMethod.DeclaringType.Name}.{callerMethod.Name}";

            if (File.Exists(filePath) == false)
            {
                Log.Debug("{userId} {fileLoader}: {filePath}", userId, "Reading file skipped (Not found)", filePath);
                return null;
            }

            if (log)
                Log.Information("{userId} {fileLoader}: {filePath}", userId, "Reading file", filePath);

            int iTry = 0;
            while (true)
            {
                var readResult = await TryReadFileAsync(filePath);
                if (readResult.exception == null)
                    return readResult.result;
                else
                {
                    iTry++;
                    if (iTry >= 5)
                    {
                        Log.Error(readResult.exception, "{userId} cannot read file {filePath}, tried 5 times", userId, filePath);
                        return null;
                    }
                }

                await Task.Delay(500);
            }
        }

        static async Task<(string result, Exception exception)> TryReadFileAsync(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fs))
                {
                    return (await reader.ReadToEndAsync(), null);
                }
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Could not read valid JSON content for file {filepath}", filePath);
                return (null, ex);
            }
            catch (IOException ex) when (ex.Message.Contains("Device or resource busy"))
            {
                return (null, ex);
            }
        }

        public async Task SaveToDiskAsync(string filePath, string content, string userId = "null", bool doLog = true)
        {
            //var doLog = filePath.Contains("_Matches_");

            //if (filePath.Contains("atches") && filePath.Contains("201910")) Debugger.Break(); 

            //var callerMethod = new StackFrame(1, true).GetMethod();
            //var context = $"{callerMethod.DeclaringType.Name}.{callerMethod.Name}";

            if (doLog)
                Log.Information("{userId} {fileLoader}: {filePath}  ({length})", "Saving file", userId, filePath, content.Length);
            else
                Log.Debug("{userId} {fileLoader}: {filePath}  ({length})", "Saving file", userId, filePath, content.Length);

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                await writer.WriteAsync(content);
            }
        }
        //public async void SaveToDisk(string filePath, string content, string userId = "null")
        //{
        //    var doLog = filePath.Contains("_Matches_");

        //    var filePathTmp = filePath + ".tmp";
        //    //var callerMethod = new StackFrame(1, true).GetMethod();
        //    //var context = $"{callerMethod.DeclaringType.Name}.{callerMethod.Name}";
        //    var context = "";

        //    Log.Debug("{userId} {fileLoader}: {filePath}  ({length})", "Saving file", userId, filePath, content.Length);
        //    if (doLog)
        //        Log.Information("{userId} {fileLoader}: {filePath}  ({length})", "Saving file", userId, filePath, content.Length);

        //    File.Delete(filePathTmp);

        //    var iTry = 0;
        //    while (File.Exists(filePathTmp) == false && iTry < 5)
        //    {
        //        if (iTry > 0)
        //        {
        //            await Task.Delay(1000);
        //            if (doLog)
        //                Log.Warning("{userId} {fileLoader}: {filePath}  ({length}) Retry #{retry}...", "Saving file", userId, filePath, content.Length, iTry);
        //        }

        //        using (FileStream fs = new FileStream(filePathTmp, FileMode.Create, FileAccess.Write, FileShare.Read))
        //        using (StreamWriter writer = new StreamWriter(fs))
        //        {
        //            writer.Write(content);
        //        }
        //        iTry++;
        //    }

        //    try
        //    {
        //        var fileFound = false;
        //        iTry = 0;
        //        while (fileFound == false && iTry < 5)
        //        {
        //            if (iTry > 0)
        //                await Task.Delay(1000);

        //            fileFound = File.Exists(filePathTmp);
        //            iTry++;
        //        }

        //        var (contentWritten, ex) = TryReadFile(filePathTmp);
        //        if (ex == null)
        //        {
        //            JsonConvert.DeserializeObject(contentWritten);
        //            // Here means that the content written is valid JSON, so we rename the file to the official data
        //            File.Move(filePathTmp, filePath, true);
        //        }
        //        else
        //        {
        //            throw ex;
        //        }
        //    }
        //    catch (JsonException ex)
        //    {
        //        Log.Error(ex, "{userId} Could not write valid JSON content for file {filepath}", userId, filePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Fatal(ex, "{userId} Could not write data for file {filepath}", userId, filePath);
        //    }
        //}
    }
}
