using Microsoft.JSInterop;
using System.Net.Http.Json;
using GamifyMe.Shared.Dtos;

namespace GamifyMe.UI.Shared.Services
{
    public class NotificationService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _jsRuntime;

        public int NewObjectivesCount { get; private set; }
        public int TotalObjectivesCount { get; private set; }

        public int NewStoreItemsCount { get; private set; }
        public int TotalStoreItemsCount { get; private set; }

        public int NewGroupsCount { get; private set; }
        public int TotalGroupsCount { get; private set; }

        public event Action? OnChange;

        public NotificationService(HttpClient http, IJSRuntime jsRuntime)
        {
            _http = http;
            _jsRuntime = jsRuntime;
        }

        public async Task LoadCountsAsync()
        {
            try
            {
                var lastVisitObjectives = await GetLastVisit("objectives");
                var lastVisitStore = await GetLastVisit("store");
                var lastVisitGroups = await GetLastVisit("groups");

                try
                {
                    var objectives = await _http.GetFromJsonAsync<List<ObjectiveDto>>("api/objectives/active");
                    if (objectives != null)
                    {
                        TotalObjectivesCount = objectives.Count;
                        NewObjectivesCount = objectives.Count(o => o.CreatedAt > lastVisitObjectives);
                    }
                }
                catch {}

                try
                {
                    var storeItems = await _http.GetFromJsonAsync<List<StoreItemDto>>("api/store/active");
                    if (storeItems != null)
                    {
                        TotalStoreItemsCount = storeItems.Count;
                        NewStoreItemsCount = storeItems.Count(i => i.CreatedAt > lastVisitStore);
                    }
                }
                catch {}

                try
                {
                    var groups = await _http.GetFromJsonAsync<List<GroupDto>>("api/groups");
                    if (groups != null)
                    {
                        TotalGroupsCount = groups.Count;
                        NewGroupsCount = groups.Count(g => g.CreatedAt > lastVisitGroups);
                    }
                }
                catch {}

                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notification counts: {ex.Message}");
            }
        }

        public async Task MarkAsSeenAsync(string type)
        {
             await SetLastVisit(type, DateTime.UtcNow);
             if (type == "objectives") NewObjectivesCount = 0;
             else if (type == "store") NewStoreItemsCount = 0;
             else if (type == "groups") NewGroupsCount = 0;

             NotifyStateChanged();
        }

        public async Task<DateTime> GetLastVisit(string type)
        {
            // IMPORTANT: If user has NEVER visited, we might want to prevent showing "All items are new".
            // Strategy: If localStorage is empty, set LastVisit to Now (first session) or MinValue?
            // If MinValue, everything is "New". Maybe overwhelming.
            // But if we set to Now, they miss current items.
            // Let's stick to MinValue (everything is new for a new user, makes sense).
            
            var str = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", $"lastVisit_{type}");
            if (DateTime.TryParse(str, out var date)) return date;
            return DateTime.MinValue;
        }

        private async Task SetLastVisit(string type, DateTime date)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"lastVisit_{type}", date.ToString("o"));
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
