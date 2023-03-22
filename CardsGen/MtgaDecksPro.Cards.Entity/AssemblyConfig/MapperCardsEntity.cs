using AutoMapper;
using Minmaxdev.Cache.Common.Service;
using Minmaxdev.Common;
using MtgaDecksPro.Cards.Entity.Exceptions;
using System.Linq;

namespace MtgaDecksPro.Cards.Entity.AssemblyConfig
{
    public class MapperCardsEntity : Profile
    {
        public MapperCardsEntity()
        {
            //CreateMap<IConfigFolderDataCards, ConfigFolder>()
            //    .ForMember(i => i.Folder, i => i.MapFrom(x => x.FolderDataCards));

            CreateMap<string, FormatInfo>().ConvertUsing<MapperStringToFormatInfo>();
        }
    }

    public class MapperStringToFormatInfo : ITypeConverter<string, FormatInfo>
    {
        private readonly ICacheHandler<FormatsAndBans> cacheHandler;

        public MapperStringToFormatInfo(
            ICacheHandler<FormatsAndBans> cacheHandler
            )
        {
            this.cacheHandler = cacheHandler;
        }

        public FormatInfo Convert(string formatRaw, FormatInfo destination, ResolutionContext context)
        {
            var cacheFormats = cacheHandler.Get().Result.FormatInfo;

            var parts = formatRaw.Split('-').Select(i => i.Substring(0, 1).ToUpper() + i.Substring(1));
            var format = string.Join(" ", parts);

            if (cacheFormats.ContainsKey(format) == false)
                throw new FormatNotFoundException(formatRaw).WithData(Minmaxdev.Common.Constants.Id, formatRaw);
            else
                return cacheFormats[format];
        }
    }
}