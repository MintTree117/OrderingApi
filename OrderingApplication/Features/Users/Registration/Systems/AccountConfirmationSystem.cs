using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Registration.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Registration.Systems;

internal sealed class AccountConfirmationSystem( UserConfigCache configCache, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    readonly UserConfigCache _configCache = configCache;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;

    internal async Task<IReply> SendEmailConfirmationLink( string email )
    {
        var userReply = await ValidateRequest( email );
        if (!userReply)
            return IReply.NotFound();
        
        var emailResult = await SendEmailConfirmationEmail( userReply.Data );
        return emailResult;
    }
    internal async Task<IReply> ConfirmEmail( ConfirmAccountEmailRequest request )
    {
        var userReply = await ValidateRequest( request.Email );
        if (!userReply)
            return IReply.NotFound();

        var confirmed = (await ConfirmEmail( userReply.Data, request.Code ))
            .OutSuccess( out var confirmReply );
        return confirmed
            ? IReply.Success()
            : IReply.Invalid( confirmReply );
    }

    async Task<Reply<UserAccount>> ValidateRequest( string email )
    {
        var userReply = await _userManager.FindByEmail( email );
        if (!userReply)
            return Reply<UserAccount>.UserNotFound();
        
        return await _userManager.IsEmailConfirmedAsync( userReply.Data )
            ? Reply<UserAccount>.Conflict( "Email is already confirmed." )
            : Reply<UserAccount>.Success( userReply.Data );
    }
    async Task<IReply> SendEmailConfirmationEmail( UserAccount user )
    {
        const string subject = "Confirm Your Email";
        var code = UserUtils.WebEncode( await _userManager.GenerateEmailConfirmationTokenAsync( user ) );
        var body = GenerateConfirmEmailBody( user, subject, code, _configCache.ConfirmEmailPage );
        var sent = _emailSender
               .SendHtmlEmail( user.Email ?? string.Empty, subject, body )
               .OutSuccess( out Reply<bool> emailResult );
        return sent
            ? IReply.Success()
            : IReply.Fail( emailResult );
    }
    async Task<IReply> ConfirmEmail( UserAccount user, string code )
    {
        var decoded = UserUtils.WebDecode( code );
        var result = await _userManager.ConfirmEmailAsync( user, decoded );
        return result.Succeeded
            ? IReply.Success()
            : IReply.Invalid( result.CombineErrors() );
    }
    static string GenerateConfirmEmailBody( UserAccount user, string subject, string code, string returnUrl )
    {
        string link = $"{returnUrl}?email={user.Email}&code={code}";
        string customBody =
            $"""
             <p>Thank you for registering with us. Please click the link below to confirm your email address:</p>
             <p>
                 <a href='{link}' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #fff; background-color: #007BFF; text-decoration: none; border-radius: 5px;'>Confirm Email</a>
             </p>
             <p>If you did not create an account, please ignore this email.</p>
             """;
        string body = UserUtils.GenerateFormattedEmail( user, subject, customBody );
        return body;
    }
}