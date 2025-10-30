using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace noMoreAzerty_front.Services;

public class VaultService
{
    public VaultService.Vault? CurrentVault { get; set; }

    private readonly HttpClient _httpClient;

    public VaultService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<List<Vault>> GetAllVaultsAsync()
    {
        var response = await _httpClient.GetAsync("api/vault");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Vault>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public class Vault
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? HashPassword { get; set; }
        public string? PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
