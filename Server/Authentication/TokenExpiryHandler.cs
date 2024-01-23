using Microsoft.AspNetCore.Components;
using System.IdentityModel.Tokens.Jwt;

namespace Server.Authentication
{
    public class TokenExpiryHandler : DelegatingHandler
    {
        private readonly AuthenticationProvider _authProvider;
        private readonly NavigationManager _navigationManager;

        public TokenExpiryHandler(AuthenticationProvider authProvider, NavigationManager navigationManager)
        {
            _authProvider = authProvider;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _authProvider.GetTokenAsync();

            if (IsTokenExpired(token.Trim('"')))
            {
                await _authProvider.LogoutAsync();
                _navigationManager.NavigateTo("/");
            }

            return await base.SendAsync(request, cancellationToken);
        }
        public bool IsTokenExpired(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token.Trim('"')) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return true;
            }

            return jwtToken.ValidTo < DateTime.UtcNow;
        }
    }
}
