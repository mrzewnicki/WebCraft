using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using JsonException = System.Text.Json.JsonException;

namespace WebCraft.Models.RequestEndpoints;

public class WebCRequestHandler<TRequestModel, TResponseModel> : BaseEndpointRequestWC
    where TRequestModel:class
    where TResponseModel:class
{
    public WebCRequestHandler(HttpClient httpClient, string url, HttpMethodType type) : base(httpClient, url, type)
    {
    }
    
    private Action<TRequestModel>? BeforeExecuteAction { get; set; } = null;
    private Action<TResponseModel>? OnSuccessAction { get; set; } = null;
    private Action<RequestErrorWebC>? OnErrorAction { get; set; } = null;
    private Dictionary<HttpStatusCode, Action<HttpResponseMessage?>>? OnHttpCodeActions { get; set; } = new();
    private Action<RequestErrorWebC>? OnExceptionAction { get; set; } = null;
    private Action? OnFinallyAction { get; set; } = null;
    public RequestResultWebC<TResponseModel> ExecuteResult { get; private set; } = null;
    
    public async Task ExecuteAsync(TRequestModel request, CancellationToken? cancellationToken = null)
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

            // Check is there any registered action on received code
            if (OnHttpCodeActions is not null && OnHttpCodeActions.Any(x => x.Key == httpResponse.StatusCode))
                foreach (var registeredAction in OnHttpCodeActions.Where(x => x.Key == httpResponse.StatusCode))
                    registeredAction.Value.Invoke(httpResponse);
            // Check is error and rise handler
            else if (!httpResponse.IsSuccessStatusCode)
                OnErrorAction?.Invoke(new()
                {
                    InvokedUrl = $"{Client.BaseAddress}{Url}",
                    RequestHttpCode = Status,
                    HttpResponseMessage = httpResponse
                });
            
            if (httpResponse?.ReasonPhrase?.ToUpper() == "NO CONTENT")
                throw new Exception("No data have been received from response");
            
            // mapping content
            mappedContent = await httpResponse?.Content?.ReadFromJsonAsync<TResponseModel>();
            
            // handle no data in response
            if (mappedContent is null)
                throw new DataException(await httpResponse.Content.ReadAsStringAsync());

            // set mapped data
            ExecuteResult = new(mappedContent);

            // success handler
            if (OnSuccessAction is not null)
                OnSuccessAction?.Invoke(mappedContent);
        }
        catch (JsonException jEx) // when mapping failed
        {
            OnExceptionAction?.Invoke(new()
            {
                InvokedUrl = $"{Client.BaseAddress}{Url}",
                RequestHttpCode = Status,
                ReceivedResponse = jEx.Data,
                Exception = jEx,
                HttpResponseMessage = httpResponse,
                MappingDataStatus = ResponseMappingStatus.IncompatibleDataModel
            });
        }
        catch (Exception e)
        {
            OnExceptionAction?.Invoke(new ()
            {
                InvokedUrl = $"{Client.BaseAddress}{Url}",
                RequestHttpCode = Status,
                ReceivedResponse = e.Data,
                Exception = e,
                HttpResponseMessage = httpResponse,
                MappingDataStatus = e.Message.ToUpper().StartsWith("NO DATA") 
                    ? ResponseMappingStatus.NoDataReceived 
                    : ResponseMappingStatus.NotReachedYet
            });
        }
        finally
        {
            OnFinallyAction?.Invoke();
        }
    }
    
    public static WebCRequestHandler<TRequestModel, TResponseModel> Create(string url, HttpClient httpClient, HttpMethodType httpMethod = HttpMethodType.Post)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentNullException.ThrowIfNull(httpClient);

        return new WebCRequestHandler<TRequestModel, TResponseModel>(httpClient, url, httpMethod);
    }
    
    public WebCRequestHandler<TRequestModel, TResponseModel> BeforeExecute(Action<TRequestModel> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        BeforeExecuteAction = action;
        return this;
    }

    public WebCRequestHandler<TRequestModel, TResponseModel> OnSuccess(Action<TResponseModel> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnSuccessAction = action;
        return this;
    }
    
    public WebCRequestHandler<TRequestModel, TResponseModel> OnError(Action<RequestErrorWebC?> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnErrorAction = action;
        return this;
    }
    
    public WebCRequestHandler<TRequestModel, TResponseModel> OnHttpCode(HttpStatusCode statusCode, Action<HttpResponseMessage?> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(OnHttpCodeActions);
        OnHttpCodeActions.Add(statusCode, action);
        return this;
    }
    
    public WebCRequestHandler<TRequestModel, TResponseModel> Catch(Action<RequestErrorWebC> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnExceptionAction = action;
        return this;
    }
    
    public WebCRequestHandler<TRequestModel, TResponseModel> Finally(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnFinallyAction = action;
        return this;
    }
}