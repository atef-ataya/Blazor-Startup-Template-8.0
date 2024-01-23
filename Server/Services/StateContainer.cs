namespace Server.Services
{
    public class StateContainer
    {
        private readonly CacheService _cacheService;

        private string? savedString;

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public StateContainer(CacheService cacheService)
        {
            _cacheService = cacheService;
        }
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
