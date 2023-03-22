using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Tracker.WPF;
using MTGAHelper.Tracker.WPF.Config;

namespace MTGAHelper.Tests.Integration
{
    [TestClass]
    public class IntegrationTestsTracker
    {
        [TestMethod]
        public void ContainerAndAutomapperConfigShouldBeValid()
        {
            var dataFolderRoot = new DataFolderRoot().FolderData;
            var container = App.CreateContainer(new ConfigModel(), dataFolderRoot);

            // Assertions
            container.Verify();
            container.GetInstance<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
