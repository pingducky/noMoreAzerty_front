using System.Text.Json;
using noMoreAzerty_dto.DTOs.Response;

namespace noMoreAzerty_front.Services;

public class UserLogsService
{
    private readonly HttpClient _httpClient;

    public UserLogsService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<List<UserListItemResponse>> GetAllUsersAsync()
    {
        var response = await _httpClient.GetAsync("api/users");

        if (!response.IsSuccessStatusCode)
            return [];

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<UserListItemResponse>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }

    public async Task<PagedLogsResponse?> GetUserLogsAsync(
        Guid userId,
        string[]? actions = null,
        int page = 1,
        int pageSize = 10)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (actions != null && actions.Length > 0)
        {
            foreach (var action in actions)
            {
                queryParams.Add($"actions={action}");
            }
        }

        var queryString = string.Join("&", queryParams);
        var response = await _httpClient.GetAsync($"api/users/{userId}/logs?{queryString}");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PagedLogsResponse>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}

public class PagedLogsResponse
{
    public List<VaultEntryHistoryItemResponse> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
