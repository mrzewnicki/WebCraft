using System.Net;

namespace WebCraft.Models;

public class BaseEndpointRequestWC
{
    public BaseEndpointRequestWC(HttpClient httpClient, string url, HttpMethodType type)
    {
        Client = httpClient;
        Url = url;
        HttpMethod = type;
    }
    
    protected HttpClient Client { get; set; } = null;
    protected string Url { get; set; }
    protected HttpMethodType HttpMethod { get; set; } = HttpMethodType.Post;
    protected HttpStatusCode Status { get; set; }
}