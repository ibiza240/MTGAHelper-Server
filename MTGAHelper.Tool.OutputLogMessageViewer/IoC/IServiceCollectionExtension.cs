using MTGAHelper.Tool.OutputLogMessageViewer.ViewModels;
using SimpleInjector;

namespace MTGAHelper.Tool.OutputLogMessageViewer.IoC
{
    public static class IServiceCollectionExtension
    {
        public static Container RegisterServicesApp(this Container container)
        {
            container.Register<MainWindow>();
            container.Register<MainWindowVM>();
            container.Register<SimulationWindow>();

            return container;
        }
    }
}
