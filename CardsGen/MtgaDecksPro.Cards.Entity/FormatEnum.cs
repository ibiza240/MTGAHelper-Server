using System;
using System.Collections.Generic;
using System.Linq;

namespace MtgaDecksPro.Cards.Entity
{
    public static class FormatEnum
    {
        public const string Unknown = Constants.Unknown;

        public const string Standard = "Standard";
        public const string TraditionalStandard = "Traditional Standard";

        public const string Historic = "Historic";
        public const string TraditionalHistoric = "Traditional Historic";

        public const string Alchemy = "Alchemy";
        public const string TraditionalAlchemy = "Traditional Alchemy";

        public const string Limited = "Limited";
        public const string Draft = "Draft";
        public const string Sealed = "Sealed";

        public const string Brawl = "Brawl";
        //public const string TraditionalBrawl = "Traditional Brawl";

        //public const string FriendlyBrawl = "Friendly Brawl";
        //public const string TraditionalHistoricBrawl = "Traditional Historic Brawl";
        public const string HistoricBrawl = "Historic Brawl";

        public const string Singleton = "Singleton";

        public const string StandardPauper = "Standard Pauper";
        public const string HistoricPauper = "Historic Pauper";

        public const string StandardArtisan = "Standard Artisan";
        public const string HistoricArtisan = "Historic Artisan";

        public const string StandardShakeup = "Standard Shakeup";
        public const string RavnicaConstructed = "Ravnica Constructed";
        public const string Cascade = "Cascade";
        public const string CascadeSingleton = "Cascade Singleton";

        public static bool IsValid(string format) => GetList(true).Any(i => i == format);

        public static ICollection<string> GetList() => GetList(false);

        private static ICollection<string> GetList(bool limited = false)
        {
            var lst = new List<string>
            {
                Standard,
                TraditionalStandard,
                Historic,
                TraditionalHistoric,
                Alchemy,
                Brawl,
                //FriendlyBrawl,
                //TraditionalBrawl,
                HistoricBrawl,
                //TraditionalHistoricBrawl,
                Singleton,
                StandardPauper,
                HistoricPauper,
                StandardArtisan,
                HistoricArtisan,
                StandardShakeup,
                RavnicaConstructed,
                Cascade,
                CascadeSingleton
            };

            if (limited)
            {
                lst.AddRange(new[] {
                Limited,
                Draft,
                Sealed,
                });
            }

            return lst;
        }

        public static string ConvertFromUrlPart(string part)
        {
            return GetList().FirstOrDefault(i => i.Equals(part.Replace("-", " "), StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsLimited(string f) => f == Limited || f == Draft || f == Sealed;
    }

    //public enum FormatEnum
    //{
    //    Unknown,

    //    Standard,
    //    TraditionalStandard,

    //    Historic,
    //    TraditionalHistoric,

    //    Draft,
    //    Sealed,

    //    Brawl,
    //    FriendlyBrawl,
    //    HistoricBrawl,

    //    Singleton,

    //    StandardPauper,
    //    HistoricPauper,

    //    StandardArtisan,
    //    HistoricArtisan,

    //    StandardShakeup,
    //    RavnicaConstructed,

    //    Cascade,
    //    CascadeSingleton,

    //    Limited,
    //}
}