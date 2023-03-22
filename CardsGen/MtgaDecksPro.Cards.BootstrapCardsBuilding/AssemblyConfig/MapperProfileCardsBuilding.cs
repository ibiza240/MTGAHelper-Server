using AutoMapper;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.AssemblyConfig.Mapper;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Mtga;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall;
using System;
using System.Linq;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.AssemblyConfig
{
    public class MapperProfileCardsBuilding : Profile
    {
        public MapperProfileCardsBuilding()
        {
            CreateMap<ScryfallModelRootObjectExtended, CardWriteable>()
                .ForMember(i => i.IdScryfall, i => i.MapFrom(x => x.id))
                .ForMember(i => i.IdArena, i => i.Ignore())
                .ForMember(i => i.IsToken, i => i.MapFrom(x => (x.type_line ?? "").StartsWith("Token") || (x.set.Length == 4 && char.ToUpperInvariant(x.set[0]) == char.ToUpperInvariant('t'))))
                .ForMember(i => i.IsPrimaryCard, i => i.MapFrom(x => MapperStatic.ConvertIsPrimary(x)))
                .ForMember(i => i.Power, i => i.MapFrom(x => Convert.ToInt32((x.power ?? "0").ToLower().Replace("+*", "").Replace("*+", "").Replace("x", "0").Replace("*", "0"))))
                .ForMember(i => i.Toughness, i => i.MapFrom(x => Convert.ToInt32((x.toughness ?? "0").ToLower().Replace("+*", "").Replace("*+", "").Replace("x", "0").Replace("*", "0"))))
                .ForMember(i => i.Flavor, i => i.MapFrom(x => (x.flavor_text ?? "").Replace("<i>", "").Replace("</i>", "")))
                .ForMember(i => i.Number, i => i.MapFrom(x => x.collector_number))
                .ForMember(i => i.ArtistCredit, i => i.MapFrom(x => x.artist))
                .ForMember(i => i.SetArena, i => i.MapFrom(x => x.set))
                .ForMember(i => i.Colors, i => i.MapFrom(x => x.colors))
                .ForMember(i => i.ColorIdentity, i => i.MapFrom(x => x.color_identity))
                .ForMember(i => i.LinkedFaces, i => i.Ignore())
                //.ForMember(i => i.ReleaseDate, i => i.MapFrom(x => DateTime.Parse(x.released_at)))
                //.ForMember(i => i.SetFullName, i => i.MapFrom(x => x.setName))
                ;

            CreateMap<MtgaDataCardsRootObjectExtended, CardWriteable>()
                .ForMember(i => i.IdScryfall, i => i.Ignore())
                .ForMember(i => i.IdArena, i => i.MapFrom(x => x.GrpId))
                .ForMember(i => i.Name, i => i.MapFrom(x => x.Name
                    .Replace("<nobr>", "")
                    .Replace("</nobr>", "")
                    .Replace("<i>", "")
                    .Replace("</i>", "")
                    .Replace("<sprite=\"SpriteSheet_MiscIcons\" name=\"arena_a\">", "")))
                .ForMember(i => i.IsToken, i => i.MapFrom(x => x.IsToken))
                .ForMember(i => i.IsPrimaryCard, i => i.MapFrom(x => x.IsSecondaryCard == false))
                .ForMember(i => i.Power, i => i.MapFrom(x => string.IsNullOrWhiteSpace(x.Power) ? "0" :
                    Convert.ToInt32(x.Power.ToLower().Replace("+*", "").Replace("*+", "").Replace("x", "0").Replace("*", "0")).ToString()
                ))
                .ForMember(i => i.Toughness, i => i.MapFrom(x => string.IsNullOrWhiteSpace(x.Toughness) ? "0" :
                    Convert.ToInt32(x.Toughness.ToLower().Replace("+*", "").Replace("*+", "").Replace("x", "0").Replace("*", "0")).ToString()
                ))
                //.ForMember(i => i.Flavor, i => i.MapFrom(x => x.Flavor.Replace("<i>", "").Replace("</i>", "")))
                .ForMember(i => i.Number, i => i.MapFrom(x => x.CollectorNumber))
                .ForMember(i => i.Rarity, i => i.ConvertUsing(new MapperRarityConverter()))
                .ForMember(i => i.ArtistCredit, i => i.MapFrom(x => x.ArtistCredit))
                .ForMember(i => i.SetArena, i => i.MapFrom(x => x.Set))
                .ForMember(i => i.Colors, i => i.ConvertUsing(new MapperColorsMtgaToChars()))
                .ForMember(i => i.ColorIdentity, i => i.ConvertUsing(new MapperColorsMtgaToChars()))
                .ForMember(i => i.LinkedFaces, i => i.MapFrom(x => x.LinkedFaces.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(c => int.Parse(c))))
            ;
        }
    }
}