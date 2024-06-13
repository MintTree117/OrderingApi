using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Utilities;

internal static class UserUtils
{
    internal static string WebEncode( string content ) =>
        WebEncoders.Base64UrlEncode( Encoding.UTF8.GetBytes( content ) );
    
    internal static string WebDecode( string content ) =>
        Encoding.UTF8.GetString( WebEncoders.Base64UrlDecode( content ) );

    internal static string GenerateFormattedEmail( UserAccount user, string subject, string body )
    {
        string html =
            $"""
                 <!DOCTYPE html>
                 <html lang='en'>
                 <head>
                     <meta charset='UTF-8'>
                     <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                     <title>{subject}</title>
                 </head>
                 <body style='font-family: Arial, sans-serif;'>
                     <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                         <h2 style='color: #333;'>{subject}</h2>
                         <p>Dear {user.UserName},</p>
                         {body}
                         <p>If you have any feedback or questions, feel free to reach out to us.</p>
                         <p>Best regards,<br/>The Team</p>
                     </div>
                 </body>
                 </html>
             """;

        return html;
    }
}