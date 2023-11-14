using System.Net.Http.Json;

namespace WebCraft.Models.RequestEndpoints;

public class EndpointRequestWC<TRequestModel, TResponseModel> : BaseEndpointRequestWC
    where TResponseModel:class
    where TRequestModel:class
{
    public EndpointRequestWC(HttpClient httpClient, string url, HttpMethodType type) : base(httpClient, url, type)
    {
    }
    
    private Action<TRequestModel>? BeforeExecuteAction { get; set; } = null;
    private Action<TResponseModel>? OnSuccessAction { get; set; } = null;
    private Action<TResponseModel?, HttpResponseMessage?>? OnErrorAction { get; set; } = null;
    private Action<Exception>? OnExceptionAction { get; set; } = null;
    private Action? OnFinallyAction { get; set; } = null;

    public RequestResultWC<TResponseModel> ExecuteResult { get; private set; }
    
    public async Task<EndpointRequestWC<TRequestModel, TResponseModel>> ExecuteAsync(TRequestModel request)
    {
        ArgumentNullException.ThrowIfNull(Client);
        ArgumentNullException.ThrowIfNull(request);
        
        BeforeExecuteAction?.Invoke(request);
        
        HttpResponseMessage? httpResponse = null;
        TResponseModel? mappedContent = null;

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
                mappedContent = await httpResponse.Content.ReadFromJsonAsync<TResponseModel>();
                if (mappedContent is null)
                    OnErrorAction?.Invoke(mappedContent, httpResponse);
                else
                {
                    ExecuteResult = new RequestResultWC<TResponseModel>()
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
    
    public static EndpointRequestWC<TRequestModel, TResponseModel> Create(string url, HttpClient httpClient, HttpMethodType httpMethod = HttpMethodType.Post)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentNullException.ThrowIfNull(httpClient);

        return new EndpointRequestWC<TRequestModel, TResponseModel>(httpClient, url, httpMethod);
    }
    
    public EndpointRequestWC<TRequestModel, TResponseModel> BeforeExecute(Action<TRequestModel> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        BeforeExecuteAction = action;
        return this;
    }

    public EndpointRequestWC<TRequestModel, TResponseModel> OnSuccess(Action<TResponseModel> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnSuccessAction = action;
        return this;
    }
    
    public EndpointRequestWC<TRequestModel, TResponseModel> OnError(Action<TResponseModel?, HttpResponseMessage?> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnErrorAction = action;
        return this;
    }
    
    public EndpointRequestWC<TRequestModel, TResponseModel> Catch(Action<Exception> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnExceptionAction = action;
        return this;
    }
    
    public EndpointRequestWC<TRequestModel, TResponseModel> Finally(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnFinallyAction = action;
        return this;
    }
}