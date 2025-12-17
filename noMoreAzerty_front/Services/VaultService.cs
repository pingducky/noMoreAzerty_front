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
        var response = await _httpClient.GetAsync("api/vault/my");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Vault>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<Vault> CreateVaultAsync(CreateVaultRequestDto createVaultRequest)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(createVaultRequest), System.Text.Encoding.UTF8, "application/json");
        // debug
        Console.WriteLine("Creating vault with request: " + JsonSerializer.Serialize(createVaultRequest));
        var response = await _httpClient.PostAsync("api/vault", jsonContent);
        // debug
        Console.WriteLine("Response status code: " + response.StatusCode);
        Console.WriteLine("Response content: " + await response.Content.ReadAsStringAsync());
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Vault>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public class Vault
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? HashPassword { get; set; }
        public string? PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVaultRequestDto // Todo : placer dans lib partagé /!\
    {
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mot de passe dérivé côté client (ex: via PBKDF2/Argon2)
        /// </summary>
        public string DerivedPassword { get; set; } = null!;

        /// <summary>
        /// Sel généré côté client
        /// </summary>
        public string PasswordSalt { get; set; } = null!;
    }

}
