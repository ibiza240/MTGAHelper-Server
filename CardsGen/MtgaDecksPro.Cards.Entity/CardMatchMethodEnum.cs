namespace MtgaDecksPro.Cards.Entity
{
    //public static class CardsBuilderConsts
    //{
    //    public static int CONFIDENCE100_MAX = 100;
    //    public static int CONFIDENCE90_VERYHIGH = 90;
    //    public static int CONFIDENCE80_HIGH = 80;
    //    public static int CONFIDENCE60_OK = 60;
    //    public static int CONFIDENCE50_MEDIUM = 50;
    //    public static int CONFIDENCE40_POOR = 40;
    //    public static int CONFIDENCE20_LOW = 20;
    //    public static int CONFIDENCE10_VERYLOW = 10;
    //    public static int CONFIDENCE0_MIN = 0;
    //}

    public enum CardMatchMethodEnum
    {
        Unknown,
        NameAndArtist,

        //NameAndSet,
        NameAndSetAndNumber,

        NameAndSetAndArtist,
        NameAndSetAndNumberAndArtist,
        GrpId,
    }
}