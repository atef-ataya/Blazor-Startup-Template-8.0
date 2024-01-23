using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using Server.Services;

namespace Server.Authentication
{
    public class ADPAuthenticationProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;

        private readonly NavigationManager navigationManager;

        public ADPAuthenticationProvider(ILocalStorageService localStorageService, NavigationManager navigationManager)
        {
            _localStorageService = localStorageService;
            this.navigationManager = navigationManager;
        }

        public async Task<string> GetTokenAsync()
        {
            return await _localStorageService.GetItem<string>("authToken");
        }


        public async Task SetTokenAsync(string token)
        {
            if (token == null)
            {
                await _localStorageService.RemoveItem("authToken");
            }
            else
            {
                await _localStorageService.SetItem("authToken", token);
            }

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task RemoveTokenAsync()
        {
            await _localStorageService.RemoveItem("authToken");
        }


        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await GetTokenAsync();

            var identity = string.IsNullOrEmpty(savedToken)
                ? new ClaimsIdentity()
                : new ClaimsIdentity(ServiceExtensions.ParseClaimsFromJwt(savedToken), "jwt");

            // Check if the token has an expiration claim ("exp") and if it is expired.
            if (identity.HasClaim(c => c.Type == JwtRegisteredClaimNames.Exp))
            {
                var expirationClaim = identity.FindFirst(c => c.Type == JwtRegisteredClaimNames.Exp);
                var expirationTime = long.Parse(expirationClaim.Value);
                var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(expirationTime).UtcDateTime;
                if (expirationDateTime < DateTime.UtcNow)
                {
                    // The token is expired Remove from localstorage
                    await RemoveTokenAsync();
                    // Redirect to login page.
                    navigationManager.NavigateTo("/");
                }
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        public void SetUserAuthenticated(string email)
        {
            var authUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, email) }, "apiauth"));
            var authState = Task.FromResult(new AuthenticationState(authUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public async Task LoginAsync(string token)
        {
            await SetTokenAsync(token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task LogoutAsync()
        {
            await RemoveTokenAsync();
            var anonUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonUser));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
