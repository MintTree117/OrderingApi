using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Registration.Types;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Registration.Systems;

internal sealed class AccountRegistrationSystem( UserConfigCache configCache, UserManager<UserAccount> userManager, IEmailSender emailSender, ILogger<AccountRegistrationSystem> logger )
    : BaseService<AccountRegistrationSystem>( logger )
{
    readonly UserConfigCache _configCache = configCache;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<IReply> RegisterAccount( RegisterAccountRequest request )
    {
        var userReply = await CreateIdentity( request );
        if (!userReply.CheckSuccess())
            return IReply.ServerError(); 
        
        var emailReply = await Utils.SendEmailConfirmationEmail( userReply.Data, _userManager, _emailSender, _configCache.ConfirmEmailPage );
        LogReplyError( emailReply );
        return IReply.Success(); // return true even if email fails
    }
    async Task<Reply<UserAccount>> CreateIdentity( RegisterAccountRequest request )
    {
        var user = new UserAccount( request.Email, request.Username, request.Phone );
        var created = await _userManager.CreateAsync( user, request.Password );
        
        LogIdentityResultError( created );
        
        return created.Succeeded
            ? Reply<UserAccount>.Success( user )
            : Reply<UserAccount>.ServerError( "Failed to create user." );
    }
}