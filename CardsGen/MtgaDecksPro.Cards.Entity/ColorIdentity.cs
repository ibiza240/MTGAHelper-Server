namespace MtgaDecksPro.Cards.Entity
{
    public struct ColorIdentity
    {
        /// <summary>
        /// Represents a color identity with 1 to 5 characters. (R, WU, WUBRG, C, etc.)
        /// </summary>
        public string ColorChars { get; set; }

        public string Name { get; set; }

        public ColorIdentity(string colorChars, string name)
        {
            ColorChars = colorChars;
            Name = name;
        }
    }
}