using System.Collections.Generic;
using System.Linq;

namespace MtgaDecksPro.Cards.Entity.Service
{
    public class ColorsSorter
    {
        private readonly Dictionary<char, int> dictOrder = new Dictionary<char, int> {
            { 'W', 1 },
            { 'U', 2 },
            { 'B', 3 },
            { 'R', 4 },
            { 'G', 5 },
            { 'C', 6 },
            { 'X', 7 },
        };

        public int GetOrder(char color) => dictOrder[color];

        public bool IsValidColor(char color) => dictOrder.ContainsKey(color);

        public string OrderColorsWubrg(string colorChars) => new string(colorChars.ToCharArray().OrderBy(i => GetOrder(i)).ToArray());
    }
}