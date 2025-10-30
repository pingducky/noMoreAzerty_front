using System.Net.Http;

namespace noMoreAzerty_front.Services
{
    public class VaultEntryService
    {
        private readonly HttpClient _httpClient;

        public VaultEntryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        // GetEntriesByVaultIdAsync
        public async Task<List<VaultEntry>> GetEntriesByVaultIdAsync(Guid vaultId)
        {
            var response = await _httpClient.GetAsync($"api/vaultentry/vault/{vaultId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<List<VaultEntry>>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            public bool? IsActive { get; set; }
            public Guid? VaultId { get; set; }
        }
    }
}
