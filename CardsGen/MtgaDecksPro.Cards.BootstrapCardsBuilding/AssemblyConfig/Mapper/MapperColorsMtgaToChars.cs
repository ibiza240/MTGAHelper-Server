using AutoMapper;
using MtgaDecksPro.Cards.Entity.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.AssemblyConfig.Mapper
{
    public class MapperColorsMtgaToChars : IValueConverter<string, ICollection<char>>
    {
        private ColorsSorter colorsOrderer = new ColorsSorter();

        public ICollection<char> Convert(string source, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source))
                return new char[0];

            return source.Split(",")
                .Select(i => int.Parse(i))
                .Select(i =>
                {
                    return i switch
                    {
                        1 => 'W',
                        2 => 'U',
                        3 => 'B',
                        4 => 'R',
                        5 => 'G',
                        _ => throw new NotImplementedException(),
                    };
                })
                .OrderBy(i => colorsOrderer.GetOrder(i))
                .ToArray();
        }
    }
}