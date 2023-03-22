namespace MtgaDecksPro.Cards.Entity.Service
{
    public class BasicLandDetectorFromTypeLine
    {
        private readonly RegexProvider regexProvider;

        public BasicLandDetectorFromTypeLine(RegexProvider regexProvider)
        {
            this.regexProvider = regexProvider;
        }

        public bool IsBasicLandFromTypeLine(string typeLine) => regexProvider.RegexBasicLand.IsMatch(typeLine);
    }
}