using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Account.Registration.Types;
using OrderingApplication.Features.Account.Utilities;
using OrderingDomain.Account;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Account.Registration.Systems;

internal sealed class AccountRegisterSystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, AccountConfirmationSystem accountSystem )
{
    readonly IdentityConfigCache _configCache = configCache;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly AccountConfirmationSystem _accountSystem = accountSystem;
    
    internal async Task<Reply<RegisterAccountResponse>> RegisterIdentity( RegisterAccountRequest registerAccount )
    {
        // VALIDATION
        Reply<bool> validationReply = IdentityUtils.ValidateRegistration( registerAccount, _configCache );
        if (!validationReply.IsSuccess)
            return RegistrationFail( validationReply );
        
        // CREATION
        Reply<UserAccount> userReply = await CreateIdentity( registerAccount );
        if (!userReply.IsSuccess)
            return RegistrationFail( userReply );
        
        // EMAIL
        Reply<bool> emailReply = await _accountSystem.SendEmailConfirmationLink( registerAccount.Email );
        return emailReply.IsSuccess
            ? RegistrationSuccess( userReply )
            : RegistrationFail( emailReply );
    }
    
    async Task<Reply<UserAccount>> CreateIdentity( RegisterAccountRequest accountRequest )
    {
        UserAccount user = new( accountRequest.Email, accountRequest.Username );
        return (await _userManager.CreateAsync( user, accountRequest.Password ))
            .Succeeds( out IdentityResult result )
                ? Reply<UserAccount>.With( user )
                : Reply<UserAccount>.None( $"Failed to create user. {result.CombineErrors()}" );
    }
    static Reply<RegisterAccountResponse> RegistrationFail( IReply? reply = null ) =>
        Reply<RegisterAccountResponse>.None( "Failed to register account." + reply );
    static Reply<RegisterAccountResponse> RegistrationSuccess( Reply<UserAccount> user ) =>
        Reply<RegisterAccountResponse>.With( RegisterAccountResponse.With( user.Data ) );
}