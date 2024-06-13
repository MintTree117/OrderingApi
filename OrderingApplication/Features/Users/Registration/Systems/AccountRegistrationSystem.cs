using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Registration.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Registration.Systems;

internal sealed class AccountRegistrationSystem( UserConfigCache configCache, UserManager<UserAccount> userManager, AccountConfirmationSystem accountSystem )
{
    readonly UserConfigCache _configCache = configCache;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly AccountConfirmationSystem _accountSystem = accountSystem;
    
    internal async Task<IReply> RegisterAccount( RegisterAccountRequest request )
    {
        var userReply = await CreateIdentity( request );
        if (!userReply.CheckSuccess())
            return IReply.ServerError( userReply );
        
        await _accountSystem.SendEmailConfirmationLink( request.Email );
        return IReply.Success(); // return true even if email fails
    }
    async Task<IReply> CreateIdentity( RegisterAccountRequest request )
    {
        var user = new UserAccount( request.Email, request.Username, request.Phone );
        var created = (await _userManager.CreateAsync( user, request.Password ))
            .SucceedsOut( out IdentityResult result );
        return created
            ? Reply<UserAccount>.Success( user )
            : Reply<UserAccount>.Failure( $"Failed to create user. {result.CombineErrors()}" );

    }
}