using AspNetCoreRateLimit;
using IdentityServer4;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib;
using MTGAHelper.Lib.CacheLoaders;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.Config.Decks;
using MTGAHelper.Lib.IoC;
using MTGAHelper.Lib.Logging;
using MTGAHelper.Lib.OutputLogParser;
using MTGAHelper.Lib.OutputLogParser.EventsSchedule;
using MTGAHelper.Lib.OutputLogParser.IoC;
using MTGAHelper.Lib.Scraping.DeckSources;
using MTGAHelper.Lib.Scraping.DeckSources.IoC;
using MTGAHelper.Lib.Scraping.NewsScraper;
using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Server.Data.CosmosDB.IoC;
using MTGAHelper.Server.Data.IoC;
using MTGAHelper.Web.Models.IoC;
using MTGAHelper.Web.UI.Extensions;
using MTGAHelper.Web.UI.IoC;
using Serilog;
using Serilog.Events;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MTGAHelper.Web.UI
{
    public class Startup
    {
        private readonly Container container = new Container();
        private readonly IConfiguration configuration;

        //public Startup(IHostingEnvironment env)
        //{
        //    //Console.WriteLine($"[{env.EnvironmentName}] env");

        //    //var builder = new ConfigurationBuilder()
        //    //    .SetBasePath(env.ContentRootPath)
        //    //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //    //    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
        //    //    .AddEnvironmentVariables();

        //    //configuration = builder.Build();
        //}

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //// This method gets called by the runtime. Use this method to add services to the container.
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services
                .AddMvcCore()
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // For IP rate limiting
            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddHttpContextAccessor();
            //services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //services.AddControllers();

            services.Configure<ConfigModelApp>(configuration.GetSection("configapp"));

            //services.AddIdentityCore<AccountModel>();
            services.AddIdentityServer(options =>
                {
                    options.Authentication.CookieSlidingExpiration = true;
                    options.Authentication.CookieLifetime = TimeSpan.FromDays(30);
                }
            )
            .AddInMemoryCaching()
            .AddClientStore<InMemoryClientStore>()
            .AddResourceStore<InMemoryResourcesStore>();

            var x = new XPS();
            var g = x.G();
            var f = x.F();

            services.AddResponseCompression(o => o.EnableForHttps = true);
            services.AddAuthorization();
            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = g.i;
                    options.ClientSecret = g.s;
                })
                .AddFacebook("Facebook", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = f.i;
                    options.ClientSecret = f.s;
                });
            //.AddTwitter("Twitter", options => {
            //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //    options.ConsumerKey = "aTkZcxBKPKry70nJ8g2YidNB5";
            //    options.ConsumerSecret = "M05v7JRyk9Wl2VwG5NKzrgGZOEWjHDKRU7r04fuFigtOVYpbED";
            //});

            // Sets up the basic configuration that for integrating Simple Injector with
            // ASP.NET Core by setting the DefaultScopedLifestyle, and setting up auto
            // cross wiring.
            services.AddSimpleInjector(container, options =>
            {
                // AddAspNetCore() wraps web requests in a Simple Injector scope and
                // allows request-scoped framework services to be resolved.
                options.AddAspNetCore()

                    // Ensure activation of a specific framework type to be created by
                    // Simple Injector instead of the built-in configuration system.
                    // All calls are optional. You can enable what you need. For instance,
                    // PageModels and TagHelpers are not needed when you build a Web API.
                    .AddControllerActivation()
                    .AddViewComponentActivation();
                //.AddPageModelActivation()
                //.AddTagHelperActivation();

                // Optionally, allow application components to depend on the non-generic
                // ILogger (Microsoft.Extensions.Logging) or IStringLocalizer
                // (Microsoft.Extensions.Localization) abstractions.
                //options.AddLogging();
                //options.AddLocalization();
            });

            InitializeContainer(container, configuration.GetSection("configapp").Get<ConfigModelApp>());
        }

        public static void InitializeContainer(Container container, ConfigModelApp config)
        {
            container.Options.ResolveUnregisteredConcreteTypes = false;

            container.RegisterServicesServerData();
            container.RegisterServicesCosmosDb();
            container.RegisterConfigLib(config)
                .RegisterFileLoaders()
                .RegisterServicesLibOutputLogParser()
                .RegisterServicesLib()
                .RegisterDecorator<IEventTypeCache, SingletonEventsScheduleManager>(Lifestyle.Singleton);
            container.RegisterSingleton<SingletonEventsScheduleManager>();
            container.RegisterServicesWebModels();
            container.RegisterServicesApp();
            container.RegisterMapperConfig();
            container.RegisterServicesDeckScrapers();
            container.RegisterServicesTracker();
            container.Register(() => Log.Logger);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSimpleInjector(container);

            var configApp = container.GetInstance<ConfigModelApp>();
            var folderLogs = configApp.FolderLogs;
            var folderData = configApp.FolderData;
            //cacheMatches.Set(new Dictionary<string, ICollection<MatchResult>>());

            var folderDataAccounts = Path.Join(folderData, "accounts");
            if (Directory.Exists(folderDataAccounts) == false)
                Directory.CreateDirectory(folderDataAccounts);

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.With(new ThreadIdEnricher())
                .Enrich.FromLogContext()
                .WriteTo.Console()

                .WriteTo.File(
                    Path.Combine(folderLogs, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] ({ThreadId}) {Message}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    Path.Combine(folderLogs, "log-errors-.txt"),
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] ({ThreadId}) {Message}{NewLine}{Exception}"
                )
                //.WriteTo.Map("userId", (userId, conf) => conf.File(
                //    Path.Combine(folderLogs, "audit", $"{userId}_audit.txt"),
                //    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] ({ThreadId}) {Message}{NewLine}{Exception}",
                //    fileSizeLimitBytes: 10485760
                //))
                .WriteTo.Map("outputLogError", (outputLogError, conf) => conf.File(
                     Path.Combine(folderLogs, $"log-outputLogErrors-.txt"),
                     rollingInterval: RollingInterval.Day,
                     outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] ({ThreadId}) {Message}{NewLine}{Exception}"
                 ))
                .WriteTo.Map("readFile", (outputLogError, conf) => conf.File(
                    Path.Combine(folderLogs, $"log-readFile-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] ({ThreadId}) {Message}{NewLine}{Exception}"
                ))

                .CreateLogger();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();
            app.UseFileServer();
            app.UseResponseCompression();

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = new List<string> { "index.html" }
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "text/plain"
            });

            app.UseRewriter(new RewriteOptions().AddRedirect("/Account/Login", "/"));

            //app.UseClientRateLimiting();

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute("default", "{controller}/{action}/{id?}");
            //});
            app.UseRouting();

            app.UseIdentityServer();
            //app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

#if DEBUG
            container.Verify();
#endif

            // Singleton initialization
            Log.Information("Loading database");
            container.GetInstance<UserDataCosmosManager>().Init().Wait();

            container.GetInstance<FilesHashManager>().Init(folderData);

            Log.Information("Loading decks and events");
            container.GetInstance<ConfigManagerDecks>().ReloadDecks();
            container.GetInstance<SingletonEventsScheduleManager>().LoadEventsFromDisk();

            // Load Calendar
            container.GetInstance<CacheSingleton<IReadOnlyCollection<ConfigModelCalendarItem>>>().Set(
                container.GetInstance<CacheLoaderCalendar>().LoadData()
            );
            container.GetInstance<CacheCalendarImageBinder>().Reload();

            // IMPORTANT: After loading allCards for DI
            //Mapper.Initialize(container.GetInstance<MapperConfigurationExpression>());
            //Mapper.AssertConfigurationIsValid();

            //decksLoaderAsync.LoadDecksFromDiskAsync();

            var decksDownloaderQueue = container.GetInstance<DecksDownloaderQueueAsync>();
            var newsDownloaderQueue = container.GetInstance<NewsDownloaderQueueAsync>();
            _ = decksDownloaderQueue.Start(CancellationToken.None);
            _ = newsDownloaderQueue.Start(CancellationToken.None);

            // Copy data to web folder
            var allFilesSrcDir = Path.GetFullPath(folderData);
            var allFilesDestDir = Path.GetFullPath(Path.Combine(configApp.FolderDlls, "wwwroot", "data"));
            Directory.CreateDirectory(allFilesDestDir);
            File.Copy(Path.Combine(allFilesSrcDir, $"{DataFileTypeEnum.AllCardsCached2}.json"), Path.Combine(allFilesDestDir, $"{DataFileTypeEnum.AllCardsCached2}.json"), true);
            File.Copy(Path.Combine(allFilesSrcDir, $"{DataFileTypeEnum.draftRatings}.json"), Path.Combine(allFilesDestDir, $"{DataFileTypeEnum.draftRatings}.json"), true);
            File.Copy(Path.Combine(allFilesSrcDir, $"{DataFileTypeEnum.ConfigResolutions}.json"), Path.Combine(allFilesDestDir, $"{DataFileTypeEnum.ConfigResolutions}.json"), true);
            File.Copy(Path.Combine(allFilesSrcDir, $"{DataFileTypeEnum.sets}.json"), Path.Combine(allFilesDestDir, $"{DataFileTypeEnum.sets}.json"), true);
        }
    }
}