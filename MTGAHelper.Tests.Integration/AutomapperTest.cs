using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MTGAHelper.Tests.Integration
{
    [TestClass]
    public class AutomapperTest
    {
        [TestMethod]
        public void ParameterlessConstructorProfilesShouldBeValid()
        {
            //var referenced = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            var assemblies = GetMTGAHelperAssemblies().ToList();

            var profiles = assemblies
                .SelectMany(a => a.GetExportedTypes()
                    .Where(t => t.IsSubclassOf(typeof(Profile))
                    && t.GetConstructor(System.Type.EmptyTypes) != null));

            var cfg = new MapperConfigurationExpression();
            foreach (var profile in profiles)
            {
                cfg.AddProfile(profile);
            }

            new MapperConfiguration(cfg).AssertConfigurationIsValid();
        }

        static IEnumerable<Assembly> GetMTGAHelperAssemblies()
        {
            var list = new List<string>();
            var stack = new Stack<Assembly>();

            stack.Push(Assembly.GetExecutingAssembly());

            do
            {
                var asm = stack.Pop();

                yield return asm;

                foreach (var reference in asm.GetReferencedAssemblies())
                    if (reference.FullName.StartsWith("MTGAHelper") && !list.Contains(reference.FullName))
                    {
                        stack.Push(Assembly.Load(reference));
                        list.Add(reference.FullName);
                    }

            }
            while (stack.Count > 0);

        }
    }
}
