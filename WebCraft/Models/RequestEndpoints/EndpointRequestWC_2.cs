using System.Net.Http.Json;

namespace WebCraft.Models.RequestEndpoints;

public class EndpointRequestWC : BaseEndpointRequestWC
{
    public EndpointRequestWC(HttpClient httpClient, string url, HttpMethodType type) : base(httpClient, url, type)
    {
    }
    
    private Action? BeforeExecuteAction { get; set; } = null;
    private Action? OnSuccessAction { get; set; } = null;
    private Action<HttpResponseMessage?>? OnErrorAction { get; set; } = null;
    private Action<Exception>? OnExceptionAction { get; set; } = null;
    private Action? OnFinallyAction { get; set; } = null;

    public RequestResultWC ExecuteResult { get; private set; }
    
    public async Task<EndpointRequestWC> ExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(Client);
        
        BeforeExecuteAction?.Invoke();
        
        HttpResponseMessage? httpResponse = null;

        try
        {
            httpResponse = HttpMethod switch
            {
                HttpMethodType.Post => await Client.PostAsync(Url, null),
                _ => throw new Exception($"Selected method is not implemented: {HttpMethod.ToString()}")
            };

            Status = httpResponse.StatusCode;

            if (httpResponse.IsSuccessStatusCode)
            {
                ExecuteResult = new RequestResultWC()
                {
                    Status = Status,
                };
                    
                OnSuccessAction?.Invoke();
            }
            else
                OnErrorAction?.Invoke(httpResponse);
        }
        catch (Exception e)
        {
            OnExceptionAction?.Invoke(e);
        }
        finally
        {
            OnFinallyAction?.Invoke();
        }

        return this;
    }
    
    public static EndpointRequestWC Create(string url, HttpClient httpClient, HttpMethodType httpMethod = HttpMethodType.Post)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentNullException.ThrowIfNull(httpClient);

        return new EndpointRequestWC(httpClient, url, httpMethod);
    }
    
    public EndpointRequestWC BeforeExecute(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        BeforeExecuteAction = action;
        return this;
    }

    public EndpointRequestWC OnSuccess(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnSuccessAction = action;
        return this;
    }
    
    public EndpointRequestWC OnError(Action<HttpResponseMessage?> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnErrorAction = action;
        return this;
    }
    
    public EndpointRequestWC Catch(Action<Exception> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnExceptionAction = action;
        return this;
    }
    
    public EndpointRequestWC Finally(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnFinallyAction = action;
        return this;
    }
}