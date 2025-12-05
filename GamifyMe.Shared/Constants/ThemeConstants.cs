namespace GamifyMe.Shared.Constants
{
    public static class ThemeConstants
    {
        public const string Default = "UI_THEME_DEFAULT";
        public const string Winter = "UI_THEME_WINTER";
        public const string Summer = "UI_THEME_SUMMER";
        public const string Autumn = "UI_THEME_AUTUMN";
        public const string Spring = "UI_THEME_SPRING";
        public const string Night = "UI_THEME_NIGHT";

        // QR Code Styles
        public const string QrStyleDefault = "QR_STYLE_DEFAULT";
        public const string QrStyleGold = "QR_STYLE_GOLD";
        public const string QrStyleInverted = "QR_STYLE_INVERTED";

        public static readonly Dictionary<string, string> FriendlyNames = new()
        {
            { Default, "Défaut" },
            { Winter, "Hiver" },
            { Summer, "Été" },
            { Autumn, "Automne" },
            { Spring, "Printemps" },
            { Night, "Mode Nuit" },
            { QrStyleDefault, "Classique" },
            { QrStyleGold, "Doré" },
            { QrStyleInverted, "Inversé" }
        };
    }
}
