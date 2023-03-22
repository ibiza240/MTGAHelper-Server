using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Services;
using MTGAHelper.Lib.CacheLoaders;
using MTGAHelper.Lib.CardProviders;
using SimpleInjector;
using System.Collections.Generic;

namespace MTGAHelper.Lib.IoC
{
    public static class SimpleInjectorRegistrations
    {
        public static Container RegisterServicesShared(this Container container)
        {
            container.RegisterConditional(
                typeof(CacheSingleton<>),
                typeof(CacheSingleton<>),
                Lifestyle.Singleton,
                c => !c.Handled);

            container.RegisterSingleton<CardRepositoryProvider>();
            container.Register(() => container.GetInstance<CardRepositoryProvider>().GetRepository());
            container.Collection.Append<Profile, MapperProfileEntity>(Lifestyle.Singleton);
            container.RegisterSingleton<AutoMapperGrpIdToCardConverter>();
            container.RegisterSingleton<Util>();
            container.RegisterSingleton<PasswordHasher>();
            container.RegisterSingleton<UtilColors>();
            container.RegisterSingleton<RawDeckConverter>();
            container.RegisterSingleton<ITimeProvider, DefaultTimeProvider>();

            container.RegisterSingleton<BasicLandIdentifier>();

            return container;
        }

        public static Container RegisterFileLoadersShared(this Container container)
        {
            container.RegisterSingleton<ICacheLoader<IReadOnlyDictionary<int, Set>>, CacheLoaderSets>();
            container.RegisterSingleton<ICacheLoader<IReadOnlyDictionary<string, DraftRatings>>, CacheLoaderDraftRatings>();
            container.RegisterSingleton<ICacheLoader<IReadOnlyDictionary<int, Card>>, CacheLoaderAllCards>();
            container.RegisterDecorator<ICacheLoader<IReadOnlyDictionary<int, Card>>, CardLoaderAddLinkedFaceCardDecorator>(Lifestyle.Singleton);

            return container;
        }
    }
}
