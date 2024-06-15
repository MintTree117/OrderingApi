using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Registration.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Registration.Systems;

internal sealed class AccountRegistrationSystem( UserManager<UserAccount> userManager, IEmailSender emailSender, ILogger<AccountRegistrationSystem> logger )
    : BaseService<AccountRegistrationSystem>( logger )
{
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<Reply<bool>> RegisterAccount( RegisterAccountRequest request )
    {
        var userReply = await CreateIdentity( request );
        if (!userReply.CheckSuccess())
            return IReply.Fail( userReply ); 
        
        var emailReply = await Utils.SendEmailConfirmationEmail( userReply.Data, _userManager, _emailSender, UserConsts.Instance.ConfirmEmailPage );
        LogIfErrorReply( emailReply );
        return IReply.Success(); // return true even if the email fails; user can execute resends
    }
    async Task<Reply<UserAccount>> CreateIdentity( RegisterAccountRequest request )
    {
        // TODO: This is a hotfix because user-manager doesnt seem to have this constraint by default...
        if (request.Username.Length is < 6 or > 24)
            return Reply<UserAccount>.BadRequest( "Username must be between 6 and 24 characters." );
        
        var user = new UserAccount( request.Email, request.Username, request.Phone );
        var created = await _userManager.CreateAsync( user, request.Password );
        
        LogIfErrorResult( created );
        
        return created.Succeeded
            ? Reply<UserAccount>.Success( user )
            : Reply<UserAccount>.BadRequest( created.CombineErrors() ); // TODO: to get validation errors for user, but may risk exposing some exceptions...
    }
}