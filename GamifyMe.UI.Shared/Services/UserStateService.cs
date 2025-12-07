namespace GamifyMe.UI.Shared.Services
{
    public class UserStateService
    {
        public event Action? OnUserDataChanged;

        public void NotifyUserDataChanged()
        {
            OnUserDataChanged?.Invoke();
        }
    }
}
