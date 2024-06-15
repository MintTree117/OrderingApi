using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Utilities;
using OrderingApplication.Features.Users.Profile.Types;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Profile;

internal sealed class AccountProfileManager( UserManager<UserAccount> userManager, ILogger<AccountProfileManager> logger )
    : BaseService<AccountProfileManager>( logger )
{
    readonly UserManager<UserAccount> _userManager = userManager;

    internal async Task<Reply<ViewProfileResponse>> ViewProfile( string userId )
    {
        var user = await _userManager.FindByIdAsync( userId );
        return user is not null
            ? Reply<ViewProfileResponse>.Success( ViewProfileResponse.With( user ) )
            : Reply<ViewProfileResponse>.UserNotFound();
    }
    internal async Task<Reply<bool>> UpdateProfile( string userId, UpdateProfileRequest update )
    {
        var user = await _userManager.FindByIdAsync( userId );
        if (user is null)
            return IReply.UserNotFound();
        
        user.UserName = update.Username;
        user.Email = update.Email;
        user.PhoneNumber = update.Phone;
        
        var updateResult = await _userManager.UpdateAsync( user );
        LogIfErrorResult( updateResult );
        return updateResult.SucceedsOut()
            ? IReply.Success()
            : IReply.ChangesNotSaved();
    }
}