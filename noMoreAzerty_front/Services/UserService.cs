using System.Net.Http.Json;

namespace noMoreAzerty_front.Services
{
    public class UserService
    {
        private readonly HttpClient _http;

        public UserService(HttpClient http)
        {
            _http = http;
        }

        public async Task InitUserAsync()
        {
            await _http.GetAsync("api/account/me");
        }
    }
}
