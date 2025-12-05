using System.Globalization;

namespace GamifyMe.UI.Shared.Helpers
{
    public static class ColorHelper
    {
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
