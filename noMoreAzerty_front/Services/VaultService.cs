using System.Net;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace noMoreAzerty_front.Services;

public class VaultService
{
    public Vault? CurrentVault { get; set; }
    public string? Password { get; set; }
    public string? Salt { get; set; }

    private readonly HttpClient _httpClient;

    public VaultService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<List<Vault>> GetAllVaultsAsync()
    {
        var response = await _httpClient.GetAsync("api/vault/my");

        if (!response.IsSuccessStatusCode)
            return [];

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Vault>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async Task<Vault?> CreateVaultAsync(CreateVaultRequestDto createVaultRequest)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(createVaultRequest), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/vault", jsonContent);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Vault>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public class Vault
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
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
