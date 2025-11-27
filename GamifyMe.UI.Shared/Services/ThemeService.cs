using GamifyMe.UI.Shared.Themes;
using MudBlazor;

namespace GamifyMe.UI.Shared.Services
{
    public class ThemeService
    {
        public MudTheme CurrentTheme { get; private set; } = CustomThemes.DefaultTheme;
        public event Action? OnChange;

        public void SetTheme(string themeCode)
        {
            CurrentTheme = CustomThemes.GetThemeByCode(themeCode);
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
