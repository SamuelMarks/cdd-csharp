namespace Generated.Client
{
usingGenerated.Models;
usingSystem.Net.Http.Json;
public class ApiClient
{
private readonly System.Net.Http.HttpClient _httpClient;

public ApiClient(System.Net.Http.HttpClienthttpClient)
{
_httpClient=httpClient;
}

public async System.Threading.Tasks.Task<string> updatePetAsync(stringbody)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PUT"),"pet");
request.Content = System.Net.Http.Json.JsonContent.Create(body);

varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> addPetAsync(stringbody)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"),"pet");
request.Content = System.Net.Http.Json.JsonContent.Create(body);

varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> findPetsByStatusAsync([Explode]stringstatus)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"),$"pet/findByStatus?status={System.Uri.EscapeDataString(System.Convert.ToString(status) ?? string.Empty)}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> findPetsByTagsAsync(System.Collections.Generic.List<string>tags)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"),$"pet/findByTags?tags={string.Join("&tags=", tags != null ? System.Linq.Enumerable.Select(tags, x => System.Uri.EscapeDataString(System.Convert.ToString(x) ?? string.Empty)) : System.Linq.Enumerable.Empty<string>())}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> getPetByIdAsync(intpetId)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"),$"pet/{System.Uri.EscapeDataString(System.Convert.ToString(petId) ?? string.Empty)}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> updatePetWithFormAsync(intpetId,stringname,stringstatus)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"),$"pet/{System.Uri.EscapeDataString(System.Convert.ToString(petId) ?? string.Empty)}?name={System.Uri.EscapeDataString(System.Convert.ToString(name) ?? string.Empty)}&status={System.Uri.EscapeDataString(System.Convert.ToString(status) ?? string.Empty)}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> deletePetAsync(stringapi_key,intpetId)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("DELETE"),$"pet/{System.Uri.EscapeDataString(System.Convert.ToString(petId) ?? string.Empty)}");
if (api_key != null) request.Headers.Add("api_key", api_key.ToString());

varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> uploadFileAsync(intpetId,stringadditionalMetadata)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"),$"pet/{System.Uri.EscapeDataString(System.Convert.ToString(petId) ?? string.Empty)}/uploadImage?additionalMetadata={System.Uri.EscapeDataString(System.Convert.ToString(additionalMetadata) ?? string.Empty)}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> getInventoryAsync()
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"),"store/inventory");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> placeOrderAsync(stringbody)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"),"store/order");
request.Content = System.Net.Http.Json.JsonContent.Create(body);

varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> getOrderByIdAsync(intorderId)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"),$"store/order/{System.Uri.EscapeDataString(System.Convert.ToString(orderId) ?? string.Empty)}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> deleteOrderAsync(intorderId)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("DELETE"),$"store/order/{System.Uri.EscapeDataString(System.Convert.ToString(orderId) ?? string.Empty)}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> createUserAsync(stringbody)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"),"user");
request.Content = System.Net.Http.Json.JsonContent.Create(body);

varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> createUsersWithListInputAsync(stringbody)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("POST"),"user/createWithList");
request.Content = System.Net.Http.Json.JsonContent.Create(body);

varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> loginUserAsync(stringusername,stringpassword)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"),$"user/login?username={System.Uri.EscapeDataString(System.Convert.ToString(username) ?? string.Empty)}&password={System.Uri.EscapeDataString(System.Convert.ToString(password) ?? string.Empty)}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> logoutUserAsync()
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"),"user/logout");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> getUserByNameAsync(stringusername)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("GET"),$"user/{System.Uri.EscapeDataString(System.Convert.ToString(username) ?? string.Empty)}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> updateUserAsync(stringusername,stringbody)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PUT"),$"user/{System.Uri.EscapeDataString(System.Convert.ToString(username) ?? string.Empty)}");
request.Content = System.Net.Http.Json.JsonContent.Create(body);

varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

public async System.Threading.Tasks.Task<string> deleteUserAsync(stringusername)
{
varrequest=newSystem.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("DELETE"),$"user/{System.Uri.EscapeDataString(System.Convert.ToString(username) ?? string.Empty)}");
varresponse=await_httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
returnawaitresponse.Content.ReadAsStringAsync();
}

}

}
