using System.Net;

namespace WebCraft.Models.RequestEndpoints;

public class RequestResultWebC<TDataModel> where TDataModel : class
{
    public RequestResultWebC(TDataModel? response)
    {
        Response = response;
    }

    public TDataModel? Response { get; set; }
}

public class RequestErrorWebC
{
    public HttpStatusCode RequestHttpCode { get; set; }
    
    public ResponseMappingStatus MappingDataStatus { get; set; }
    
    public object? ReceivedResponse { get; set; }
    public string InvokedUrl { get; set; } = string.Empty;
    public Exception? Exception { get; set; } = null;
    
    public HttpResponseMessage? HttpResponseMessage { get; set; } = null;
}

public enum ResponseMappingStatus
{
    IncompatibleDataModel,
    NoDataReceived,
    NotReachedYet
}