using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Registration.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Registration.Systems;

internal sealed class AccountRegistrationSystem( AccountConfig config, UserManager<UserAccount> userManager, AccountConfirmationSystem accountSystem )
{
    readonly AccountConfig _config = config;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly AccountConfirmationSystem _accountSystem = accountSystem;
    
    internal async Task<Reply<RegisterAccountResponse>> RegisterAccount( RegisterAccountRequest registerAccount )
    {
        // CREATION
        Reply<UserAccount> userReply = await CreateIdentity( registerAccount );
        if (!userReply.Succeeded)
            return RegistrationFail( userReply );
        
        // EMAIL
        Reply<bool> emailReply = await _accountSystem.SendEmailConfirmationLink( registerAccount.Email );
        return emailReply.Succeeded
            ? RegistrationSuccess( userReply )
            : RegistrationFail( emailReply );
    }
    
    async Task<Reply<UserAccount>> CreateIdentity( RegisterAccountRequest accountRequest )
    {
        UserAccount user = new( accountRequest.Email, accountRequest.Username );
        return (await _userManager.CreateAsync( user, accountRequest.Password ))
            .Succeeds( out IdentityResult result )
                ? Reply<UserAccount>.Success( user )
                : Reply<UserAccount>.Failure( $"Failed to create user. {result.CombineErrors()}" );
    }
    static Reply<RegisterAccountResponse> RegistrationFail( IReply? reply = null ) =>
        Reply<RegisterAccountResponse>.Failure( "Failed to register account." + reply );
    static Reply<RegisterAccountResponse> RegistrationSuccess( Reply<UserAccount> user ) =>
        Reply<RegisterAccountResponse>.Success( RegisterAccountResponse.With( user.Data ) );
}