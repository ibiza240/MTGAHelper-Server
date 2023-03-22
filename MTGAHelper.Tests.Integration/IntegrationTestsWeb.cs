using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Web.UI;
using SimpleInjector;

namespace MTGAHelper.Tests.Integration
{
    [TestClass]
    public class IntegrationTestsWeb
    {
        [TestMethod]
        public void ContainerAndAutomapperConfigShouldBeValid()
        {
            var dataFolderRoot = new DataFolderRoot().FolderData;
            var container = new Container();
            Startup.InitializeContainer(container,
                new ConfigModelApp
                {
                    FolderData = dataFolderRoot,
                    InfoBySet = new Dictionary<string, ConfigModelSetInfo>()
                });
            // cross-wired and resolved by Microsoft DI container, so register a dummy here
            container.RegisterSingleton(typeof(IOptionsMonitor<>), typeof(DummyOptionsMonitor<>));

            // Assertions
            container.Verify();
            container.GetInstance<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
        }

        class DummyOptionsMonitor<T> : IOptionsMonitor<T>
        {
            public T Get(string name)
            {
                throw new NotImplementedException();
            }

            public IDisposable OnChange(Action<T, string> listener)
            {
                throw new NotImplementedException();
            }

            public T CurrentValue { get; }
        }
    }
}
