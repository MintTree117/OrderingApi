namespace OrderingApplication.Middleware;

public class UpdateCookieMiddleware( RequestDelegate next )
{
    readonly RequestDelegate _next = next;

    public async Task InvokeAsync( HttpContext context )
    {
        string? authCookie = null;
        
        bool skip =
            context.User.Identity is null ||
            !context.User.Identity.IsAuthenticated ||
            !context.Request.Cookies.TryGetValue( "AuthCookie", out authCookie );
        
        if (skip)
            await _next( context );

        var newExpiryDate = DateTime.UtcNow.AddHours( 24 );
        var maxExpiryDate = DateTime.UtcNow.AddDays( 7 );
        
        if (newExpiryDate > maxExpiryDate)
            newExpiryDate = maxExpiryDate;

        var cookieOptions = new CookieOptions {
            HttpOnly = true,
            Expires = newExpiryDate
        };

        context.Response.Cookies.Append( "AuthCookie", authCookie ?? string.Empty, cookieOptions );
        await _next( context );
    }
}