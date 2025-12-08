using System.Globalization;

namespace GamifyMe.UI.Shared.Helpers
{
    public static class ColorHelper
    {
        public static List<MudBlazor.Utilities.MudColor> Palette => new List<MudBlazor.Utilities.MudColor>
        {
            // Greyscale (5)
            "#FFFFFF", "#E0E0E0", "#9E9E9E", "#424242", "#000000",

            // Red & Pink (6)
            "#EF9A9A", "#F44336", "#C62828", // Red
            "#F48FB1", "#E91E63", "#AD1457", // Pink

            // Purple & Indigo (5)
            "#9C27B0", "#6A1B9A", // Purple
            "#673AB7", "#3F51B5", "#1A237E", // Indigo/Deep Blue

            // Blue & Cyan (5)
            "#2196F3", "#1565C0", // Blue
            "#03A9F4", "#00BCD4", "#006064", // Cyan

            // Teal & Green (5)
            "#009688", // Teal
            "#4CAF50", "#2E7D32", // Green
            "#8BC34A", "#CDDC39", // Light Green/Lime

            // Yellow & Orange (5)
            "#FFEB3B", "#FFC107", // Yellow/Amber
            "#FF9800", "#EF6C00", "#FF5722", // Orange

            // Earth & Neutral (4)
            "#795548", "#4E342E", // Brown
            "#607D8B", "#37474F"  // Blue Grey
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
