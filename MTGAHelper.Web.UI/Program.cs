using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTGAHelper.Web.UI
{
    public class Program
    {
        public static void Tool_GetAllInvalidJsonFiles()
        {
            var folder = @"D:\repos\MTGAHelper\BAK_PROD_20190829\data\configusers";

            var invalidFiles = new List<string>();
            var files = Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories).ToArray();
            var total = files.Length;
            int i = 0;
            foreach (var f in files)
            {
                try
                {
                    var t = JsonConvert.DeserializeObject(File.ReadAllText(f));
                }
                catch (Exception ex)
                {
                    invalidFiles.Add(f);
                }

                i++;
                if (i % 1000 == 0)
                    Console.WriteLine($"{i} / {total} ({invalidFiles.Count} invalids)");
            }

            var outputPath = @"D:\repos\MTGAHelper\invalid.json";
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(invalidFiles));
        }
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);

                    options.Limits.MaxConcurrentConnections = 50;
                    options.Limits.MaxConcurrentUpgradedConnections = 50;
                    //options.Limits.MaxRequestBodySize = 100 * 1024;
                    //options.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    //options.Limits.MinResponseDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                })
                .ConfigureAppConfiguration((builder, conf) =>
                {
                    var env = builder.HostingEnvironment;

                    conf.SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
            host.Run();

            Log.CloseAndFlush();
        }
    }
}
