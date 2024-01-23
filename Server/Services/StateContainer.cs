namespace Server.Services
{
    public class StateContainer
    {
        private string? savedString;

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
        public string SavedString
        {
            get => savedString ?? string.Empty;
            set
            {
                savedString = value;
                NotifyStateChanged();
            }
        }

        public void ResetState()
        {

        }
    }
}
