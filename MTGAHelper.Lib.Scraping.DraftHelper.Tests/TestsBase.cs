﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using AutoMapper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.IO.Writer.WriterDeckTypes;
using MTGAHelper.Lib.IoC;
using MTGAHelper.Lib.OutputLogParser.IoC;
using MTGAHelper.Server.Data.CosmosDB.IoC;
using MTGAHelper.Server.Data.IoC;
using SimpleInjector;
using System.Linq;

namespace MTGAHelper.Lib.Scraping.DraftHelper.Tests
{
    [TestClass]
    public class TestsBase
    {
        public CsvConfiguration ConfigCsv { get; private set; } = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        protected string userTest = ConfigModelUser.USER_LOCAL;

        protected static IServiceProvider provider;

        //static protected IUtilLib util;
        protected static IWriterDeckMtga writerDeckMtga;

        protected static ICardRepository allCards;
        //protected static ICollection<MtgaMappingItem> mappings;

        protected static string root;
        protected static string folderData;
        protected static string folderDecks;

        protected ScraperType scraperType = new ScraperType("");

        protected Dictionary<string, IDeck> Decks { get; set; }
        protected ICollection<CardWithAmount> Collection { get; set; }
        protected Dictionary<string, Card> Cards { get; set; }
        protected ICollection<dynamic> Expected { get; set; }
        protected ICollection<dynamic> ExpectedByDeck { get; set; }
        protected Dictionary<string, InfoCardMissingSummary[]> ExpectedMissingCardsSummary { get; set; }
        protected InfoCardInDeck[] ExpectedAllDecksCards { get; set; }

        [AssemblyInitialize]
        public static void Setup(TestContext testContext)
        {
            //var services = new ServiceCollection()
            //    .RegisterServicesLib("configapp.json", "configdecks.json", "configusers", "configdeckuserscrapers.json");

            root = Path.Combine(Directory.GetCurrentDirectory(), "../../../..");
            folderData = Path.Combine(root, @"MTGAHelper.UnitTests\Data");

            folderDecks = Path.Combine(root, "Decks");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("configapp.json", optional: false, reloadOnChange: true)
                .Build();

            var container = new Container
            {
                Options = { EnableAutoVerification = false }
            }
                .RegisterServicesServerData()
                .RegisterServicesCosmosDb()
                .RegisterConfigLib(config.Get<ConfigModelApp>())
                .RegisterFileLoaders()
                .RegisterServicesLib();

            provider = container;
            writerDeckMtga = provider.GetRequiredService<IWriterDeckMtga>();

            allCards = provider.GetRequiredService<ICardRepository>();
        }
    }
}