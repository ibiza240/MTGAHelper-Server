using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Entity
{
    public class FormatsFileModel : Dictionary<string, DataByFormat>
    {
    }

    public record DataByFormat
    {
        public string Description { get; set; }
        public bool Active { get; set; }
        public bool Obsolete { get; set; }
        public int BestOf { get; set; }

        /// <summary>
        /// Lists of sets that are legal for the format (special values: Historical and Standard)
        /// </summary>
        public ICollection<string> Sets { get; set; }

        public ICollection<string> Banned { get; set; }
        public ICollection<string> Suspended { get; set; }
    }
}