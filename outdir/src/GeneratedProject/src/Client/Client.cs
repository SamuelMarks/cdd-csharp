namespace Generated.Client
{
using Generated.Models;
using System.Net.Http.Json;
public class ApiClient
{
private readonly System.Net.Http.HttpClient _httpClient;

public ApiClient(System.Net.Http.HttpClient httpClient)
{
_httpClient = httpClient;
}

public async System.Threading.Tasks.Task<string> updatePetAsync(string body)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PUT"), "pet");
request.Content = System.Net.Http.Json.JsonContent.Create(body);
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> addPetAsync(string body)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), "pet");
request.Content = System.Net.Http.Json.JsonContent.Create(body);
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> findPetsByStatusAsync([Explode] string status)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), $"pet/findByStatus?status={System.Uri.EscapeDataString(System.Convert.ToString(status) ?? string.Empty)}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> findPetsByTagsAsync(System.Collections.Generic.List<string> tags)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), $"pet/findByTags?tags={string.Join("&tags=", tags != null ? System.Linq.Enumerable.Select(tags, x => System.Uri.EscapeDataString(System.Convert.ToString(x) ?? string.Empty)) : System.Linq.Enumerable.Empty<string>())}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> getPetByIdAsync(int petId)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), $"pet/{System.Uri.EscapeDataString(System.Convert.ToString(petId) ?? string.Empty)}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> updatePetWithFormAsync(int petId, string name, string status)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), $"pet/{System.Uri.EscapeDataString(System.Convert.ToString(petId) ?? string.Empty)}?name={System.Uri.EscapeDataString(System.Convert.ToString(name) ?? string.Empty)}&status={System.Uri.EscapeDataString(System.Convert.ToString(status) ?? string.Empty)}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> deletePetAsync(string api_key, int petId)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("DELETE"), $"pet/{System.Uri.EscapeDataString(System.Convert.ToString(petId) ?? string.Empty)}");
if (api_key != null)
    request.Headers.Add("api_key", api_key.ToString());
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> uploadFileAsync(int petId, string additionalMetadata)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), $"pet/{System.Uri.EscapeDataString(System.Convert.ToString(petId) ?? string.Empty)}/uploadImage?additionalMetadata={System.Uri.EscapeDataString(System.Convert.ToString(additionalMetadata) ?? string.Empty)}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> getInventoryAsync()
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), "store/inventory");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> placeOrderAsync(string body)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), "store/order");
request.Content = System.Net.Http.Json.JsonContent.Create(body);
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> getOrderByIdAsync(int orderId)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), $"store/order/{System.Uri.EscapeDataString(System.Convert.ToString(orderId) ?? string.Empty)}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> deleteOrderAsync(int orderId)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("DELETE"), $"store/order/{System.Uri.EscapeDataString(System.Convert.ToString(orderId) ?? string.Empty)}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> createUserAsync(string body)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), "user");
request.Content = System.Net.Http.Json.JsonContent.Create(body);
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> createUsersWithListInputAsync(string body)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), "user/createWithList");
request.Content = System.Net.Http.Json.JsonContent.Create(body);
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> loginUserAsync(string username, string password)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), $"user/login?username={System.Uri.EscapeDataString(System.Convert.ToString(username) ?? string.Empty)}&password={System.Uri.EscapeDataString(System.Convert.ToString(password) ?? string.Empty)}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> logoutUserAsync()
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), "user/logout");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> getUserByNameAsync(string username)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"), $"user/{System.Uri.EscapeDataString(System.Convert.ToString(username) ?? string.Empty)}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> updateUserAsync(string username, string body)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PUT"), $"user/{System.Uri.EscapeDataString(System.Convert.ToString(username) ?? string.Empty)}");
request.Content = System.Net.Http.Json.JsonContent.Create(body);
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> deleteUserAsync(string username)
{
var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("DELETE"), $"user/{System.Uri.EscapeDataString(System.Convert.ToString(username) ?? string.Empty)}");
var response = await _httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
return await response.Content.ReadAsStringAsync();
}

}

}
