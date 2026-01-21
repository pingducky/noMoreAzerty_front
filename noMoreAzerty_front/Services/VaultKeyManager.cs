using Microsoft.JSInterop;
using System.Text.Json;

namespace noMoreAzerty_front.Services;

/// <summary>
/// Gestionnaire de clés
/// 
/// ARCHITECTURE :
/// - KEY_DERIVATION : Dérivée du MDP via PBKDF2 (chiffre les données)
/// - KEY_STORAGE : Clé RNG éphémère (stockée API session)
/// - ENCRYPTED_KEY : KEY_DERIVATION chiffrée (stockée front)
/// 
/// SÉCURITÉ :
/// Protection XSS : Nécessite front + API compromis
/// Protection serveur : KEY_STORAGE seule ne suffit pas
/// API ne peut jamais déchiffrer les données
/// </summary>
public class VaultKeyManager
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _httpClient;

    // KEY_DERIVATION chiffrée (stockée front)
    private string? _encryptedKey;
    private string? _ivStorage;
    private string? _tagStorage;

    // État
    private Guid? _currentVaultId;
    private bool _isUnlocked;

    public VaultKeyManager(IJSRuntime js, IHttpClientFactory httpClientFactory)
    {
        _js = js;
        _httpClient = httpClientFactory.CreateClient("API");
    }

    /// <summary>
    /// Déverrouille un coffre
    /// </summary>
    public async Task<bool> UnlockVaultAsync(Guid vaultId, string password, string salt)
    {
        try
        {
            // Vérifier le mot de passe
            var isValid = await VerifyPasswordAsync(vaultId, password);
            if (!isValid)
                return false;

            // Générer KEY_DERIVATION depuis le mot de passe (PBKDF2)
            var keyDerivation = await _js.InvokeAsync<string>("deriveKeyString", password, salt, 100000);

            // Générer KEY_STORAGE (RNG)
            var keyStorage = await _js.InvokeAsync<string>("generateRandomKey", 32);
            var ivStorage = await _js.InvokeAsync<string>("generateRandomBytes", 12);

            // Chiffrer KEY_DERIVATION avec KEY_STORAGE
            var encrypted = await _js.InvokeAsync<Dictionary<string, string>>(
                "encryptAesGcm",
                keyStorage,
                "",
                keyDerivation
            );

            // Stocker ENCRYPTED_KEY localement
            _encryptedKey = encrypted["ciphertext"];
            _tagStorage = encrypted["tag"];
            _ivStorage = encrypted["iv"];

            // Envoyer KEY_STORAGE à l'API pour stockage session
            var storeSuccess = await StoreKeyStorageAsync(vaultId, keyStorage);
            if (!storeSuccess)
                return false;

            _currentVaultId = vaultId;
            _isUnlocked = true;

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unlock failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Récupère KEY_DERIVATION pour chiffrer/déchiffrer
    /// </summary>
    public async Task<string?> GetDerivationKeyAsync(Guid vaultId)
    {
        if (!_isUnlocked || _currentVaultId != vaultId)
            throw new UnauthorizedAccessException("Vault is locked");

        if (string.IsNullOrEmpty(_encryptedKey))
            throw new InvalidOperationException("No encrypted key stored");

        // Récupérer KEY_STORAGE depuis l'API
        var keyStorage = await GetKeyStorageAsync(vaultId);
        if (keyStorage == null)
        {
            Clear();
            throw new UnauthorizedAccessException("Session expired");
        }

        // Déchiffrer ENCRYPTED_KEY avec KEY_STORAGE
        var keyDerivation = await _js.InvokeAsync<string>(
            "decryptAesGcm",
            keyStorage,
            "",
            _ivStorage,
            _tagStorage,
            _encryptedKey,
            100000
        );

        return keyDerivation;
    }

    /// <summary>
    /// Vérifie le mot de passe
    /// </summary>
    private async Task<bool> VerifyPasswordAsync(Guid vaultId, string password)
    {
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(new { Password = password }),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync($"api/vaults/{vaultId}/unlock", jsonContent);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Envoie KEY_STORAGE à l'API pour stockage en session
    /// </summary>
    private async Task<bool> StoreKeyStorageAsync(Guid vaultId, string keyStorage)
    {
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(new { KeyStorage = keyStorage }),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync($"api/vaults/{vaultId}/session/store-key", jsonContent);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Récupère KEY_STORAGE depuis l'API
    /// </summary>
    private async Task<string?> GetKeyStorageAsync(Guid vaultId)
    {
        var response = await _httpClient.GetAsync($"api/vaults/{vaultId}/session/key");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<KeyStorageResponse>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.KeyStorage;
    }

    /// <summary>
    /// Verrouille le coffre et nettoie
    /// </summary>
    public async Task LockVaultAsync()
    {
        if (_currentVaultId.HasValue)
        {
            await _httpClient.DeleteAsync($"api/vaults/{_currentVaultId}/session/key");
        }

        Clear();
    }

    /// <summary>
    /// Nettoie la mémoire locale
    /// </summary>
    public void Clear()
    {
        _encryptedKey = null;
        _ivStorage = null;
        _tagStorage = null;
        _currentVaultId = null;
        _isUnlocked = false;
    }

    public bool IsVaultUnlocked(Guid vaultId) => _isUnlocked && _currentVaultId == vaultId;

    private record KeyStorageResponse(string KeyStorage);
}