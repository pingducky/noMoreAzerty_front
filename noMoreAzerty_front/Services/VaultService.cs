using System.Text.Json;
using noMoreAzerty_dto.DTOs.Request;
using noMoreAzerty_dto.DTOs.Response;

namespace noMoreAzerty_front.Services;

public class VaultService
{
    public GetVaultResponse? CurrentVault { get; set; }
    public string? Password { get; set; }
    public string? Salt { get; set; }

    private readonly HttpClient _httpClient;

    public VaultService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<List<GetVaultResponse>> GetAllVaultsAsync()
    {
        var response = await _httpClient.GetAsync("api/vault/my");

        if (!response.IsSuccessStatusCode)
            return [];

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<GetVaultResponse>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async Task<List<VaultUser>> GetVaultUsersAsync(Guid vaultId)
    {
        // TODO : Route à créer dans l'API
        var response = await _httpClient.GetAsync($"api/vault/{vaultId}/users");

        if (!response.IsSuccessStatusCode)
            return [];

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<VaultUser>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
    }

    public async Task<GetVaultResponse?> CreateVaultAsync(CreateVaultRequest createVaultRequest)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(createVaultRequest), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/vault", jsonContent);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetVaultResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async Task<GetVaultResponse?> UpdateVaultAsync(Guid vaultId, UpdateVaultNameRequest updateRequest)
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
        return JsonSerializer.Deserialize<GetVaultResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> DeleteVaultAsync(Guid vaultId)
    {
        var response = await _httpClient.DeleteAsync($"api/vault/{vaultId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> VerifyVaultPasswordAsync(Guid vaultId, string password)
    {
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(new { Password = password }),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync($"api/vaults/{vaultId}/entries/access", jsonContent);

        if (!response.IsSuccessStatusCode)
            return false;

        var content = await response.Content.ReadAsStringAsync();

        // Désérialiser directement le bool
        var result = JsonSerializer.Deserialize<bool>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result;
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

    public class UpdateVaultNameRequest
    {
        public string Name { get; set; } = null!;
    }
}
