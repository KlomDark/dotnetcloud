using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DotNetCloud.Server.UI.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly NavigationManager _nav;
        public AuthService(HttpClient http, IJSRuntime js, NavigationManager nav)
        {
            _http = http;
            _js = js;
            _nav = nav;
        }

        public async Task<bool> LoginAsync(string userName, string password, bool canUseJsInterop, SessionService? session = null)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", new { UserName = userName, Password = password });
            if (!response.IsSuccessStatusCode) return false;
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            if (result?.Token != null && canUseJsInterop)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "jwt", result.Token);
                if (session != null)
                {
                    ParseAndSetSession(result.Token, session);
                }
                return true;
            }
            return false;
        }

        public void ParseAndSetSession(string jwt, SessionService session)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            session.Username = token.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name" || c.Type == "sub")?.Value;
            session.Email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            // Extract all role claims
            session.Roles = token.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();
        }

        public async Task LogoutAsync(bool canUseJsInterop)
        {
            if (canUseJsInterop)
                await _js.InvokeVoidAsync("localStorage.removeItem", "jwt");
        }

        public async Task<string?> GetTokenAsync(bool canUseJsInterop)
        {
            if (canUseJsInterop)
                return await _js.InvokeAsync<string>("localStorage.getItem", "jwt");
            return null;
        }

        public List<KeyValuePair<string, string>> GetClaimsFromToken(string jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt)) return new();
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token.Claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToList();
        }

        public class LoginResult
        {
            public string Token { get; set; }
        }
    }
}
