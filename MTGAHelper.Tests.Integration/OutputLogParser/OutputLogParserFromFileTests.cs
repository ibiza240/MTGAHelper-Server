using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.IoC;
using MTGAHelper.Lib.OutputLogParser;
using MTGAHelper.Lib.OutputLogParser.IoC;
using MTGAHelper.Lib.OutputLogParser.Models;
using Newtonsoft.Json;

namespace MTGAHelper.Tests.Integration.OutputLogParser
{
    [TestClass]
    public class OutputLogParserFromFileTests
    {
        private readonly string logFolder;
        private readonly Func<ReaderMtgaOutputLog> getReader;

        public OutputLogParserFromFileTests()
        {
            var root = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
            logFolder = Path.Combine(root, @"OutputLogParser\FullLogFiles");

            var container = new SimpleInjector.Container();
            container.RegisterSingleton<IDataPath, DataFolderRoot>();
            container.RegisterServicesLibOutputLogParser();
            container.RegisterFileLoaders();
            container.RegisterMapperConfig();
            container.RegisterServicesShared();
            getReader = container.GetInstance<ReaderMtgaOutputLog>;
        }

        [TestMethod]
        public async Task ProcessedFiles_should_MatchExpectedTypes()
        {
            foreach (var logFilePath in Directory.EnumerateFiles(logFolder, "Player*.log", SearchOption.TopDirectoryOnly))
            {
                var expectedTypesPath = logFilePath.Replace("Player", "expected_types");
                var expectedTypes = JsonConvert.DeserializeObject<string[]>(await File.ReadAllTextAsync(expectedTypesPath));

                var expectedReaderUsagePath = logFilePath.Replace("Player", "expected_readerUsage");
                var expectedReaderUsage = JsonConvert.DeserializeObject<ConverterUsage[]>(await File.ReadAllTextAsync(expectedReaderUsagePath));

                using var stream = File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var messages = (await getReader().ProcessIntoMessagesAsync("test", stream)).ToArray();
                var types = messages.Select(r => r.GetType().Name).ToArray();

                // Read the file 2 times...oh well
                using var stream2 = File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var logReadersUsage = (await getReader().LoadFileContent("", stream2)).result2.LogReadersUsage;
                var json2 = JsonConvert.SerializeObject(logReadersUsage);

                // Assert
                messages.Length.Should().Be(expectedTypes.Length, expectedTypesPath);
                foreach (var (message, expectedType) in messages.Zip(expectedTypes))
                {
                    var type = message.GetType().Name;
                    type.Should().Be(expectedType, $"{message.Part} ({expectedTypesPath})");
                }

                logReadersUsage.Count.Should().Be(expectedReaderUsage.Length, expectedReaderUsagePath);
                foreach (var (actual, expected) in logReadersUsage.Zip(expectedReaderUsage))
                {
                    actual.Should().BeEquivalentTo(expected, $"{actual.LogTextKey} ({expectedReaderUsagePath})");
                }
            }
        }

        [TestMethod, Ignore]
        public async Task Tool_ProcessFilesMakeExpectedTypes()
        {
            foreach (var logFilePath in Directory.EnumerateFiles(logFolder, "Player*.log", SearchOption.TopDirectoryOnly))
            {
                await using var stream = File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var result = await getReader().ProcessIntoMessagesAsync("test", stream);
                var types = result.Select(r => r.GetType().Name).ToArray();
                var expectedFilePath = logFilePath.Replace("Player", "expected_types");
                await File.WriteAllTextAsync(expectedFilePath, JsonConvert.SerializeObject(types, Formatting.Indented));

                await using var stream2 = File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var logReadersUsage = (await getReader().LoadFileContent("", stream2)).result2.LogReadersUsage;
                var json2 = JsonConvert.SerializeObject(logReadersUsage, Formatting.Indented);
                var expectedUsage = logFilePath.Replace("Player", "expected_readerUsage");
                await File.WriteAllTextAsync(expectedUsage, json2);
            }
        }
    }
}