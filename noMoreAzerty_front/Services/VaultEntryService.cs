using Microsoft.JSInterop;
using System.Net.Http;
using System.Text.Json;
using static noMoreAzerty_front.Services.VaultService;

namespace noMoreAzerty_front.Services
{
    public class VaultEntryService
    {
        public List<VaultEntry>? Entries { get; set; }

        private readonly HttpClient _httpClient;

        public VaultEntryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<List<VaultEntry>?> GetEntriesByVaultIdAsync(VaultAccessRequestDto request, Guid vaultId)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"api/vaults/{vaultId}/entries/access", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<VaultEntry>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<VaultEntry>();
        }

        public async Task<VaultEntry?> CreateEntryAsync(CreateVaultEntryRequestDto createVaultEntryRequest, Guid vaultId)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(createVaultEntryRequest), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"api/vaults/{vaultId}/entries/create", jsonContent);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VaultEntry>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
            
        public async Task<bool> DeleteEntryAsync(Guid vaultId, Guid entryId)
        {           
            var response = await _httpClient.DeleteAsync(
                $"api/vaults/{vaultId}/entries/{entryId}");

            return response.IsSuccessStatusCode;
        }

        public async Task<CreateVaultEntryRequestDto> EncryptVaultEntryAsync(
            string password, string salt, string title, string username, string passwordValue, string url, string commentary, IJSRuntime js)
        {
            var titleResult = await js.InvokeAsync<Dictionary<string, string>>("encryptAesGcm", password, salt, title);
            var usernameResult = await js.InvokeAsync<Dictionary<string, string>>("encryptAesGcm", password, salt, username);
            var passwordResult = await js.InvokeAsync<Dictionary<string, string>>("encryptAesGcm", password, salt, passwordValue);
            var urlResult = await js.InvokeAsync<Dictionary<string, string>>("encryptAesGcm", password, salt, url);
            var commentaryResult = await js.InvokeAsync<Dictionary<string, string>>("encryptAesGcm", password, salt, commentary);

            return new CreateVaultEntryRequestDto
            {
                CipherTitle = titleResult["ciphertext"],
                TitleIV = titleResult["iv"],
                TitleTag = titleResult["tag"],
                CipherUsername = usernameResult["ciphertext"],
                UsernameIV = usernameResult["iv"],
                UsernameTag = usernameResult["tag"],
                CipherPassword = passwordResult["ciphertext"],
                PasswordIV = passwordResult["iv"],
                PasswordTag = passwordResult["tag"],
                CipherUrl = urlResult["ciphertext"],
                UrlIV = urlResult["iv"],
                UrlTag = urlResult["tag"],
                CipherCommentary = commentaryResult["ciphertext"],
                ComentaryIV = commentaryResult["iv"],
                ComentaryTag = commentaryResult["tag"]
            };
        }

        public async Task<VaultEntry> DecryptVaultEntryAsync(VaultEntry entry, string password, string salt, IJSRuntime js)
        {
            // Appelle "decryptAesGcm" côté JS pour chaque champ
            // À adapter selon ta structure
            entry.CipherTitle = await js.InvokeAsync<string>("decryptAesGcm", password, salt, entry.CipherTitle, entry.TitleIV, entry.TitleTag);
            entry.CipherUsername = await js.InvokeAsync<string>("decryptAesGcm", password, salt, entry.CipherUsername, entry.UsernameIV, entry.UsernameTag);
            entry.CipherPassword = await js.InvokeAsync<string>("decryptAesGcm", password, salt, entry.CipherPassword, entry.PasswordIV, entry.PasswordTag);
            entry.CipherUrl = await js.InvokeAsync<string>("decryptAesGcm", password, salt, entry.CipherUrl, entry.UrlIV, entry.UrlTag);
            entry.CipherCommentary = await js.InvokeAsync<string>("decryptAesGcm", password, salt, entry.CipherCommentary, entry.ComentaryIV, entry.ComentaryTag);
            return entry;
        }

        public class CreateVaultEntryRequestDto
        {
            public string? CipherTitle { get; set; }
            public string? TitleIV { get; set; }
            public string? TitleTag { get; set; }
            public string? CipherUsername { get; set; }
            public string? UsernameIV { get; set; }
            public string? UsernameTag { get; set; }
            public string? CipherPassword { get; set; }
            public string? PasswordIV { get; set; }
            public string? PasswordTag { get; set; }
            public string? CipherUrl { get; set; }
            public string? UrlIV { get; set; }
            public string? UrlTag { get; set; }
            public string? CipherCommentary { get; set; }
            public string? ComentaryIV { get; set; }
            public string? ComentaryTag { get; set; }
        }

        public class VaultEntry
        {
            public Guid Id { get; set; }
            public string? CipherTitle { get; set; }
            public string? TitleIV { get; set; }
            public string? TitleTag { get; set; }
            public string? CipherUsername { get; set; }
            public string? UsernameIV { get; set; }
            public string? UsernameTag { get; set; }
            public string? CipherPassword { get; set; }
            public string? PasswordIV { get; set; }
            public string? PasswordTag { get; set; }
            public string? CipherUrl { get; set; }
            public string? UrlIV { get; set; }
            public string? UrlTag { get; set; }
            public string? CipherCommentary { get; set; }
            public string? ComentaryIV { get; set; }
            public string? ComentaryTag { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public class VaultAccessRequestDto
        {
            /// <summary>
            /// Mot de passe en clair envoyé par le client
            /// </summary>
            public string Password { get; set; } = null!;
        }
    }
}
