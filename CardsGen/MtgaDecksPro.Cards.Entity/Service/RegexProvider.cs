using System.Text.RegularExpressions;

namespace MtgaDecksPro.Cards.Entity.Service
{
    public class RegexProvider
    {
        internal readonly Regex RegexBasicLand = new Regex("^Basic.*?Land", RegexOptions.Compiled);
    }
}