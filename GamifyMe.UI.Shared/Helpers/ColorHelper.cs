using System.Globalization;

namespace GamifyMe.UI.Shared.Helpers
{
    public static class ColorHelper
    {
        public static List<MudBlazor.Utilities.MudColor> Palette => new List<MudBlazor.Utilities.MudColor>
        {
            // Row 1: Warm & Pinks
            "#F44336", "#E91E63", "#9C27B0", "#673AB7", "#3F51B5",
            // Row 2: Blues & Teals
            "#2196F3", "#03A9F4", "#00BCD4", "#009688", "#4CAF50",
            // Row 3: Greens & Yellows
            "#8BC34A", "#CDDC39", "#FFEB3B", "#FFC107", "#FF9800",
            // Row 4: Oranges & Neutrals
            "#FF5722", "#795548", "#9E9E9E", "#607D8B", "#000000",
            // Row 5: White & Darks
            "#FFFFFF", "#1A237E", "#B71C1C", "#1B5E20", "#F57F17"
        };

        public static string GetContrastColor(string? hexColor)
        {
            if (string.IsNullOrEmpty(hexColor)) return "black";

            try
            {
                // Remove # if present
                hexColor = hexColor.TrimStart('#');

                // Parse RGB
                if (hexColor.Length == 6)
                {
                    int r = int.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
                    int g = int.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber);
                    int b = int.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber);

                    // Calculate luminance
                    // Formula: 0.299*R + 0.587*G + 0.114*B
                    double luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

                    // Return black for bright colors, white for dark colors
                    return luminance > 0.5 ? "black" : "white";
                }
            }
            catch
            {
                // Fallback
                return "black";
            }

            return "black";
        }
    }
}
