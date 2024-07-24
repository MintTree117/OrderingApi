using System.Collections.Specialized;
using System.Net.Http.Json;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Http;

internal sealed class HttpService( IConfiguration configuration, ILogger<HttpService> logger ) : IHttpService
{
    const string ErrorMessage = "An exception occurred while trying to make an http request.";
    readonly HttpClient _http = GetHttpClient( configuration );
    readonly ILogger<HttpService> _logger = logger;

    public async Task<Reply<T>> TryGet<T>( string apiPath, Dictionary<string, object>? parameters = null, string? authToken = null )
    {
        try {
            SetAuthHttpHeader( authToken );
            string url = GetQueryParameters( apiPath, parameters );
            HttpResponseMessage httpResponse = await _http.GetAsync( url );
            return await HandleHttpObjResponse<T>( httpResponse );
        }
        catch ( Exception e ) {
            return HandleHttpObjException<T>( e, apiPath );
        }
    }
    public async Task<Reply<T>> TryPost<T>( string apiPath, object? body = null, string? authToken = null )
    {
        try {
            SetAuthHttpHeader( authToken );
            HttpResponseMessage httpResponse = await _http.PostAsJsonAsync( apiPath, body );
            return await HandleHttpObjResponse<T>( httpResponse );
        }
        catch ( Exception e ) {
            return HandleHttpObjException<T>( e, apiPath );
        }
    }
    public async Task<Reply<T>> TryPut<T>( string apiPath, object? body = null, string? authToken = null )
    {
        try {
            SetAuthHttpHeader( authToken );
            HttpResponseMessage httpResponse = await _http.PutAsJsonAsync( apiPath, body );
            return await HandleHttpObjResponse<T>( httpResponse );
        }
        catch ( Exception e ) {
            return HandleHttpObjException<T>( e, apiPath );
        }
    }
    public async Task<Reply<T>> TryDelete<T>( string apiPath, Dictionary<string, object>? parameters = null, string? authToken = null )
    {
        try {
            SetAuthHttpHeader( authToken );
            string url = GetQueryParameters( apiPath, parameters );
            HttpResponseMessage httpResponse = await _http.DeleteAsync( url );
            return await HandleHttpObjResponse<T>( httpResponse );
        }
        catch ( Exception e ) {
            return HandleHttpObjException<T>( e, apiPath );
        }
    }
    
    static string GetQueryParameters( string apiPath, Dictionary<string, object>? parameters )
    {
        if (parameters is null)
            return apiPath;

        NameValueCollection query = HttpUtility.ParseQueryString( string.Empty );

        foreach ( KeyValuePair<string, object> param in parameters ) 
            query[param.Key] = param.Value.ToString();

        return $"{apiPath}?{query}";
    }
    static async Task<Reply<T>> HandleHttpObjResponse<T>( HttpResponseMessage httpResponse )
    {
        if (httpResponse.IsSuccessStatusCode) {
            var httpContent = await httpResponse.Content.ReadFromJsonAsync<T>();
            return httpContent is not null
                ? Reply<T>.Success( httpContent )
                : Reply<T>.Failure( "No data returned from http request." );
        }

        string errorContent = await httpResponse.Content.ReadAsStringAsync();
        Console.WriteLine( $"An exception was thrown during an http request : {errorContent}" );
        return Reply<T>.Failure( $"An exception was thrown during an http request : {errorContent}" );
    }
    
    Reply<T> HandleHttpObjException<T>( Exception e, string requestUrl )
    { 
        _logger.LogError( e, e.Message );
        return Reply<T>.Failure( $"{ErrorMessage} : {requestUrl}" );
    }
    void SetAuthHttpHeader( string? token )
    {
        _http.DefaultRequestHeaders.Authorization = !string.IsNullOrWhiteSpace( token )
            ? new System.Net.Http.Headers.AuthenticationHeaderValue( "Bearer", token )
            : null;
    }

    static HttpClient GetHttpClient( IConfiguration config ) =>
        new( new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes( 3 ) } ) {
            BaseAddress = new Uri( config["BaseUrl"] ?? throw new Exception( "Failed to get BaseUrl from IConfiguration in HttpService!" ) )
        };
}