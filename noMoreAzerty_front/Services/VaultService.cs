using System.Net.Http.Json;
using System.Text.Json;

namespace noMoreAzerty_front.Services
{
    public class VaultService
    {
        public Vault? CurrentVault { get; set; }

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
            return JsonSerializer.Deserialize<List<Vault>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Vault>();
        }

        public async Task<List<Vault>> GetMyVaultsAsync()
        {
            var response = await _httpClient.GetAsync("api/vault/my");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Vault>>() ?? new List<Vault>();
        }

        public async Task<List<Vault>> GetSharedVaultsAsync()
        {
            var response = await _httpClient.GetAsync("api/vault/shared");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Vault>>() ?? new List<Vault>();
        }

        public async Task<Vault?> CreateVaultAsync(string name, string password)
        {
            var payload = new { Name = name, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/vault/create", payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Vault>();
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
}
