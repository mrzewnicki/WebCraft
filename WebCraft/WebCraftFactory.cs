using WebCraft.Models;
using WebCraft.Models.RequestEndpoints;

namespace WebCraft;

public static class WebCraftFactory
{
    public static class HttpRequest
    {
        /// <summary>
        /// Send HTTP request containing generic payload. Also receive response from server and map automatically to generic model 
        /// </summary>
        /// <param name="url">Url that will be combined with base address configured in http client</param>
        /// <param name="httpClient">Http client configuration</param>
        /// <param name="httpMethod">Http method that will be used to invoke request</param>
        /// <typeparam name="TRequestModel">Data model which will be send to server</typeparam>
        /// <typeparam name="TResponseModel">Data model which will be received from server</typeparam>
        /// <returns></returns>
        public static EndpointRequestWC<TRequestModel, TResponseModel> Prepare<TRequestModel, TResponseModel>(string url, HttpClient httpClient, HttpMethodType httpMethod = HttpMethodType.Post)
            where TRequestModel : class
            where TResponseModel : class
        {
            ArgumentException.ThrowIfNullOrEmpty(url);
            ArgumentNullException.ThrowIfNull(httpClient);

            return new EndpointRequestWC<TRequestModel, TResponseModel>(httpClient, url, httpMethod);
        }
        
        /// <summary>
        /// Send HTTP request containing generic payload if you use <see cref="ExecuteAsync(TModel request)"/>.
        /// Receive response from server and map automatically to generic model if you use ExecuteAsync().
        /// </summary>
        /// <param name="url">Url that will be combined with base address configured in http client</param>
        /// <param name="httpClient">Http client configuration</param>
        /// <param name="httpMethod">Http method that will be used to invoke request</param>
        /// <typeparam name="TModel">Data model which will be use to receive or send data, depending on method you invoke</typeparam>
        /// <returns></returns>
        public static EndpointRequestWC<TModel> Prepare<TModel>(string url, HttpClient httpClient, HttpMethodType httpMethod = HttpMethodType.Post)
            where TModel : class
        {
            ArgumentException.ThrowIfNullOrEmpty(url);
            ArgumentNullException.ThrowIfNull(httpClient);

            return new EndpointRequestWC<TModel>(httpClient, url, httpMethod);
        }
        
        public static EndpointRequestWC Prepare(string url, HttpClient httpClient, HttpMethodType httpMethod = HttpMethodType.Post)
        {
            ArgumentException.ThrowIfNullOrEmpty(url);
            ArgumentNullException.ThrowIfNull(httpClient);

            return new EndpointRequestWC(httpClient, url, httpMethod);
        }
    }
}