using MudBlazor;
using GamifyMe.Shared.Constants;

namespace GamifyMe.UI.Shared.Themes
{
    public static class CustomThemes
    {
        public static MudTheme DefaultTheme = new MudTheme();

        public static MudTheme WinterTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.Blue.Default,
                Secondary = Colors.Cyan.Accent3,
                AppbarBackground = Colors.Blue.Darken2,
                Background = Colors.Gray.Lighten5
            }
        };

        public static MudTheme SummerTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.Orange.Default,
                Secondary = Colors.Yellow.Accent4,
                AppbarBackground = Colors.Orange.Darken2,
                Background = Colors.Amber.Lighten5
            }
        };

        public static MudTheme AutumnTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.Brown.Default,
                Secondary = Colors.DeepOrange.Accent3,
                AppbarBackground = Colors.Brown.Darken2,
                Background = Colors.Orange.Lighten5
            }
        };

        public static MudTheme SpringTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.Green.Default,
                Secondary = Colors.Pink.Accent2,
                AppbarBackground = Colors.Green.Darken2,
                Background = Colors.LightGreen.Lighten5
            }
        };

        public static MudTheme NightTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.Yellow.Accent4,
                Secondary = Colors.Yellow.Darken1,
                AppbarBackground = "#0f0f0f", // Almost black
                Background = "#121212",      // Very dark background
                Surface = "#1e1e1e",         // Slightly lighter for cards/surfaces
                TextPrimary = Colors.Shades.White,
                TextSecondary = Colors.Gray.Lighten2,
                ActionDefault = Colors.Yellow.Accent2,
                DrawerBackground = "#1a1a1a",
                DrawerText = Colors.Shades.White,
                DrawerIcon = Colors.Yellow.Accent2
            }
        };

        public static MudTheme GetThemeByCode(string code)
        {
            return code switch
            {
                ThemeConstants.Winter => WinterTheme,
                ThemeConstants.Summer => SummerTheme,
                ThemeConstants.Autumn => AutumnTheme,
                ThemeConstants.Spring => SpringTheme,
                ThemeConstants.Night => NightTheme,
                _ => DefaultTheme
            };
        }
    }
}
