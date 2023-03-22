using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.IoC;
using MTGAHelper.Lib.OutputLogParser.InMatchTracking;
using MTGAHelper.Lib.OutputLogParser.IoC;
using MTGAHelper.Tool.OutputLogMessageViewer.IoC;
using SimpleInjector;

namespace MTGAHelper.Tool.OutputLogMessageViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        IConfiguration configuration;

        protected override void OnStartup(StartupEventArgs e)
        {
            var folderForConfigAndLog = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(folderForConfigAndLog, "appsettings.json"), optional: false, reloadOnChange: true)
                .Build();


            var config = configuration.Get<ConfigModelApp>();

            var container = new Container();
            container.Options.ResolveUnregisteredConcreteTypes = false;
            container.RegisterInstance(config);
            container.RegisterInstance<IDataPath>(config);
            container.RegisterServicesLibOutputLogParser();
            container.RegisterFileLoaders();
            container.RegisterServicesShared();
            container.RegisterServicesApp();
            container.RegisterMapperConfig();

            //container.RegisterSingleton<InGameTracker2>();
            container.RegisterServicesTracker();

            container.Verify();

            var mainWindow = container.GetInstance<MainWindow>();
            mainWindow.Show();
        }
    }
}
