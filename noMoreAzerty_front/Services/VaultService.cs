using noMoreAzerty_front.Handlers;
using System.Text.Json;

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

    public async Task<List<VaultUser>> GetVaultUsersAsync(Guid vaultId)
    {
        var response = await _httpClient.GetAsync($"api/vault/{vaultId}/users");

        if (!response.IsSuccessStatusCode)
            return [];

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<VaultUser>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
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

    public async Task<Vault?> UpdateVaultAsync(Guid vaultId, UpdateVaultNameRequest updateRequest)
    {
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PutAsync($"api/vault/{vaultId}", jsonContent);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Vault>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> DeleteVaultAsync(Guid vaultId)
    {
        var response = await _httpClient.DeleteAsync($"api/vault/{vaultId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ShareVaultAsync(Guid vaultId, string userEmail)
    {
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(new { Email = userEmail }),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync($"api/vault/{vaultId}/share", jsonContent);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveUserFromVaultAsync(Guid vaultId, Guid userId)
    {
        var response = await _httpClient.DeleteAsync($"api/vault/{vaultId}/users/{userId}");

        return response.IsSuccessStatusCode;
    }

    // Classe pour représenter un utilisateur ayant accès au coffre
    public class VaultUser
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public bool IsOwner { get; set; } // Pour empêcher de supprimer le propriétaire
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

    public class UpdateVaultNameRequest
    {
        public string Name { get; set; } = null!;
    }
}
