using System.Net;

namespace WebCraft.Models;

public class RequestResultWC<TResponseModel> where TResponseModel : class
{
    public HttpStatusCode Status { get; set; }
    public TResponseModel Response { get; set; }

    public bool IsError => Status != HttpStatusCode.OK;
    public bool IsSuccess => Status == HttpStatusCode.OK;
}

public class RequestResultWC
{
    public HttpStatusCode Status { get; set; }

    public bool IsError => Status != HttpStatusCode.OK;
    public bool IsSuccess => Status == HttpStatusCode.OK;
}