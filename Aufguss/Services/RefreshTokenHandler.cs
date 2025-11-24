using System.Net;
using System.Net.Http.Headers;

namespace Aufguss.Services
{
    

    public class RefreshTokenHandler : DelegatingHandler
    {
        private readonly TokenRefreshService _refreshService;

        public RefreshTokenHandler(TokenRefreshService refreshService)
        {
            _refreshService = refreshService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Attach token
            var token = await _refreshService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            // If unauthorized → attempt refresh
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool refreshed = await _refreshService.TryRefreshAsync();
                if (refreshed)
                {
                    // Retry the request with new token
                    token = await _refreshService.GetAccessTokenAsync();
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    return await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }
    }

}
