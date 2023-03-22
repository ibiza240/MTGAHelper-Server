using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MTGAHelper.Tracker.WPF.Tools;
using System;

namespace MTGAHelper.Tracker.WPF.Models
{
    public class CardWithAmountWpf : CardWpf
    {
        public virtual int Amount { get; set; }
    }

    public class CardWpf : BasicModel
    {
        public int ArenaId { get; set; }

        public string Name { get; set; }

        public string Rarity { get; set; }

        public string ImageCardUrl { get; set; }

        public string ImageArtUrl { get; set; }

        public ICollection<string> Colors { get; set; } = Array.Empty<string>();

        public ICollection<string> ColorIdentity { get; set; } = Array.Empty<string>();

        public string ManaCost { get; set; }

        public int Cmc { get; set; }

        public string Type { get; set; }

        private static readonly Regex RegexCmcImages = new Regex(@"{([^}]+)}", RegexOptions.Compiled);

        public IEnumerable<string> CmcImages
        {
            get
            {
                MatchCollection matches = RegexCmcImages.Matches(ManaCost ?? "");

                if (matches.Count == 0) return Array.Empty<string>();

                var ret = matches
                    .Select(i => i.Value.Replace("{", "").Replace("}", "").Replace("/", ""))
                    .Select(i => $"https://www.mtgahelper.com/images/manaIcons/{i}.png")
                    .ToArray();

                return ret;
            }
        }
    }
}
