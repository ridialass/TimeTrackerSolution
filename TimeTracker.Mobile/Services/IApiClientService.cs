using System.Net.Http;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _client;
        public ApiClientService(HttpClient client) => _client = client;

        public Task<HttpResponseMessage> GetAsync(string requestUri)
            => _client.GetAsync(requestUri);

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
            => _client.PostAsync(requestUri, content);
    }
}
