using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Identity.Types.Registration;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Services;

internal sealed class RegistrationSystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, EmailConfirmationSystem emailSystem )
{
    readonly IdentityConfigCache _configCache = configCache;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly EmailConfirmationSystem _emailSystem = emailSystem;
    
    internal async Task<Reply<RegisterReply>> RegisterIdentity( RegisterRequest register )
    {
        // VALIDATION
        Reply<bool> validationReply = IdentityValidationUtils.ValidateRegistration( register, _configCache );
        if (!validationReply.IsSuccess)
            return RegistrationFail( validationReply );
        
        // CREATION
        Reply<UserAccount> userReply = await CreateIdentity( register );
        if (!userReply.IsSuccess)
            return RegistrationFail( userReply );
        
        // EMAIL
        Reply<bool> emailReply = await _emailSystem.SendConfirmationLink( register.Email );
        return emailReply.IsSuccess
            ? RegistrationSuccess( userReply )
            : RegistrationFail( emailReply );
    }
    
    async Task<Reply<UserAccount>> CreateIdentity( RegisterRequest request )
    {
        UserAccount user = new( request.Email, request.Username );
        return (await _userManager.CreateAsync( user, request.Password ))
            .Succeeds( out IdentityResult result )
                ? Reply<UserAccount>.With( user )
                : Reply<UserAccount>.None( $"Failed to create user. {result.CombineErrors()}" );
    }
    static Reply<RegisterReply> RegistrationFail( IReply? reply = null ) =>
        Reply<RegisterReply>.None( "Failed to register account." + reply );
    static Reply<RegisterReply> RegistrationSuccess( Reply<UserAccount> user ) =>
        Reply<RegisterReply>.With( RegisterReply.With( user.Data ) );
}