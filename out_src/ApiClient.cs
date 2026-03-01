namespace Generated.Client
{
    public class ApiClient
    {
        private readonly System.Net.Http.HttpClient _httpClient;
        public ApiClient(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async System.Threading.Tasks.Task<string> getPetsAsync()
        {
            var response = await _httpClient.GetAsync("/pets");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}