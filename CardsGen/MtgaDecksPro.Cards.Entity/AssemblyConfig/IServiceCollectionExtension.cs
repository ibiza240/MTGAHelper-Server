using Microsoft.Extensions.DependencyInjection;
using MtgaDecksPro.Cards.Entity.Service;

namespace MtgaDecksPro.Cards.Entity.AssemblyConfig
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection RegisterServicesCardsEntity(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MapperCardsEntity))

                .AddSingleton<RegexProvider>()
                .AddSingleton<BasicLandDetectorFromTypeLine>()
                .AddSingleton<ColorsSorter>()
                ;
        }
    }
}