using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Server.Utilities
{
    public class RestClient
    {
        private readonly ILogger<RestClient> _logger;

        public RestClient(ILogger<RestClient> logger)
        {
            _logger = logger;
        }

        public async Task<T> CallService<T>(string token, string url, HttpMethod method, object body = null)
        {
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            using (HttpClient client = new HttpClient(clientHandler))
            {
                HttpRequestMessage msg = new HttpRequestMessage(method, url);

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Add("X-AUTH-TOKEN", token);
                }

                if (body != null)
                {
                    if (body != null)
                    {
                        switch (body)
                        {
                            case MultipartFormDataContent multipartContent:
                                msg.Content = multipartContent;
                                break;
                            default:
                                var bodyJson = JsonConvert.SerializeObject(body);
                                msg.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                                break;
                        }
                    }
                }
                var content = string.Empty;

                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                try
                {
                    var result = await client.SendAsync(msg);
                    content = await result.Content.ReadAsStringAsync();

                    if (!result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"HTTP Request Failed: {result.StatusCode}, Content: {content}");

                        throw new HttpRequestException($"Request to {url} failed with status code {result.StatusCode}");
                    }

                    var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                    return JsonConvert.DeserializeObject<T>(content, settings);
                }
                catch (ForbiddenException ex)
                {
                    _logger.LogError($"Forbidden Exception: {ex.Message}");
                    throw new ForbiddenException($"Forbidden: {ex.Message}");
                }
                catch (NotFoundException ex)
                {
                    _logger.LogError($"{ex.Message}");
                    throw new NotFoundException($"Not Found: {ex.Message}");
                }
                catch (UnauthorizedException ex)
                {
                    _logger.LogError($"{ex.Message}");
                    throw new UnauthorizedException($"Unauthorized: {ex.Message}");
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError($"An HTTP request exception occurred: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An Exception happened: {ex.Message}");
                    throw new Exception("An error has occurred: ", ex);
                }
            }
        }
    }

    public class ForbiddenException : ApplicationException
    {
        public ForbiddenException(string message) : base(message) { }

        public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UnauthorizedException : ApplicationException
    {
        public UnauthorizedException(string message) : base(message) { }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
