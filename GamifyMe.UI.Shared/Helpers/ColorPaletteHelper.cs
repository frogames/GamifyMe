using MudBlazor;
using MudBlazor.Utilities;

namespace GamifyMe.UI.Shared.Helpers
{
    public static class ColorPaletteHelper
    {
        public static IEnumerable<MudColor> Palette256 => _palette256;

        private static readonly List<MudColor> _palette256 = Generate256Colors();

        private static List<MudColor> Generate256Colors()
        {
            var colors = new List<MudColor>();

            // Standard colors
            colors.Add(new MudColor("#000000"));
            colors.Add(new MudColor("#FFFFFF"));
            colors.Add(new MudColor("#FF0000"));
            colors.Add(new MudColor("#00FF00"));
            colors.Add(new MudColor("#0000FF"));
            colors.Add(new MudColor("#FFFF00"));
            colors.Add(new MudColor("#00FFFF"));
            colors.Add(new MudColor("#FF00FF"));

            // Generate a 6x6x6 color cube (216 colors)
            int[] values = { 0x00, 0x33, 0x66, 0x99, 0xCC, 0xFF };
            foreach (var r in values)
            {
                foreach (var g in values)
                {
                    foreach (var b in values)
                    {
                        colors.Add(new MudColor($"#{r:X2}{g:X2}{b:X2}"));
                    }
                }
            }

            // Grayscale (remaining to fill or just useful ones)
            for (int i = 0; i < 24; i++)
            {
                int val = 0x10 + (i * 10);
                if (val > 0xFF) val = 0xFF;
                colors.Add(new MudColor($"#{val:X2}{val:X2}{val:X2}"));
            }

            return colors.Distinct().ToList();
        }
    }
}
