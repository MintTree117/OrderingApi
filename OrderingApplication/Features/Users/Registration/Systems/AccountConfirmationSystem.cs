using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Registration.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Registration.Systems;

internal sealed class AccountConfirmationSystem( UserManager<UserAccount> userManager, IEmailSender emailSender, ILogger<AccountConfirmationSystem> logger )
    : BaseService<AccountConfirmationSystem>( logger )
{
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;

    internal async Task<IReply> SendEmailConfirmationLink( string email )
    {
        var userReply = await ValidateRequest( email );
        if (!userReply)
            return IReply.NotFound();
        
        var emailReply = await Utils.SendEmailConfirmationEmail( userReply.Data, _userManager, _emailSender, UserConsts.Instance.ConfirmEmailPage );
        LogReplyError( emailReply );
        return emailReply;
    }
    internal async Task<IReply> ConfirmEmail( ConfirmAccountEmailRequest request )
    {
        var userReply = await ValidateRequest( request.Email );
        if (!userReply)
            return IReply.NotFound();

        var confirmed = await ConfirmEmail( userReply.Data, request.Code );
        return confirmed.CheckSuccess()
            ? IReply.Success()
            : IReply.Invalid( "Failed to confirm email." );
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
    async Task<IReply> ConfirmEmail( UserAccount user, string code )
    {
        var decoded = UserUtils.WebDecode( code );
        var result = await _userManager.ConfirmEmailAsync( user, decoded );
        LogIdentityResultError( result );
        return result.Succeeded
            ? IReply.Success()
            : IReply.Invalid( result.CombineErrors() );
    }
}