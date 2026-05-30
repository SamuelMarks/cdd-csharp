namespace Generated.Client
{
    using Generated.Models;
    using System.Net.Http.Json;
    public class ApiClient
    {
        private readonly System.Net.Http.HttpClient _httpClient;
        public ApiClient(System.Net.Http.HttpClient httpClient)
        {
            _httpClient=httpClient;
            
        }
        
        /// <response code="200">
        
        /// A list of pets
        
        /// </response>
        public async System.Threading.Tasks.Task<string>getPetsAsync()
        {
            var request=new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"),"pets");
            var response=await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
            
        }
        
    }
    
}
