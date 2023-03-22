using System;

namespace MtgaDecksPro.Cards.Entity
{
    public struct Set
    {
        public int MtgaId { get; set; }
        public string? CodeArena { get; set; }
        public string CodeScryfall { get; set; }
        public string Name { get; set; }
        public int NbCards { get; set; }
        public string PromoCardNumber { get; set; }
        public bool PromoCardIsStyle { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}