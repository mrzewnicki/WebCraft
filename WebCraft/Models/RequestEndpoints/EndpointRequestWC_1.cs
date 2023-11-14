using System.Net.Http.Json;

namespace WebCraft.Models.RequestEndpoints;

public class EndpointRequestWC<TModel> : BaseEndpointRequestWC
    where TModel:class
{
    public EndpointRequestWC(HttpClient httpClient, string url, HttpMethodType type) : base(httpClient, url, type)
    {
    }
    
    private Action<TModel?>? BeforeExecuteAction { get; set; } = null;
    private Action<TModel?>? OnSuccessAction { get; set; } = null;
    private Action<TModel?, HttpResponseMessage?>? OnErrorAction { get; set; } = null;
    private Action<Exception>? OnExceptionAction { get; set; } = null;
    private Action? OnFinallyAction { get; set; } = null;

    public RequestResultWC<TModel?> ExecuteResult { get; private set; }
    
    public async Task<EndpointRequestWC<TModel>> ExecuteAsync(TModel request)
    {
        ArgumentNullException.ThrowIfNull(Client);
        ArgumentNullException.ThrowIfNull(request);
        
        BeforeExecuteAction?.Invoke(request);
        
        HttpResponseMessage? httpResponse = null;

        try
        {
            httpResponse = HttpMethod switch
            {
                HttpMethodType.Post => await Client.PostAsJsonAsync(Url, request),
                _ => throw new Exception($"Selected method is not implemented: {HttpMethod.ToString()}")
            };

            Status = httpResponse.StatusCode;

            if (httpResponse.IsSuccessStatusCode)
            {
                ExecuteResult = new RequestResultWC<TModel>()
                {
                    Status = Status,
                };
                    
                OnSuccessAction?.Invoke(null);
            }
            else
                OnErrorAction?.Invoke(null, httpResponse);
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
    
    public async Task<EndpointRequestWC<TModel>> ExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(Client);
        
        BeforeExecuteAction?.Invoke(null);
        
        HttpResponseMessage? httpResponse = null;
        TModel? mappedContent = null;

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
                mappedContent = await httpResponse.Content.ReadFromJsonAsync<TModel>();
                if (mappedContent is null)
                    OnErrorAction?.Invoke(mappedContent, httpResponse);
                else
                {
                    ExecuteResult = new RequestResultWC<TModel>()
                    {
                        Status = Status,
                        Response = mappedContent
                    };
                    
                    OnSuccessAction?.Invoke(mappedContent);
                }
            }
            else
                OnErrorAction?.Invoke(null, httpResponse);
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
    
    public static EndpointRequestWC<TModel> Create(string url, HttpClient httpClient, HttpMethodType httpMethod = HttpMethodType.Post)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentNullException.ThrowIfNull(httpClient);

        return new EndpointRequestWC<TModel>(httpClient, url, httpMethod);
    }
    
    public EndpointRequestWC<TModel> BeforeExecute(Action<TModel> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        BeforeExecuteAction = action;
        return this;
    }

    public EndpointRequestWC<TModel> OnSuccess(Action<TModel?> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnSuccessAction = action;
        return this;
    }
    
    public EndpointRequestWC<TModel> OnError(Action<TModel?, HttpResponseMessage?> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnErrorAction = action;
        return this;
    }
    
    public EndpointRequestWC<TModel> Catch(Action<Exception> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnExceptionAction = action;
        return this;
    }
    
    public EndpointRequestWC<TModel> Finally(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnFinallyAction = action;
        return this;
    }
}