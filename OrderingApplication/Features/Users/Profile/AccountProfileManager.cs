using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Utilities;
using OrderingApplication.Features.Users.Profile.Types;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Profile;

internal sealed class AccountProfileManager( UserManager<UserAccount> users )
{
    readonly UserManager<UserAccount> _users = users;

    internal async Task<Reply<ViewProfileResponse>> ViewProfile( string userId )
    {
        UserAccount? user = await _users.FindByIdAsync( userId );
        return user is not null
            ? Reply<ViewProfileResponse>.Success( ViewProfileResponse.With( user ) )
            : Reply<ViewProfileResponse>.UserNotFound();
    }
    internal async Task<Reply<bool>> UpdateProfile( string userId, UpdateProfileRequest update )
    {
        UserAccount? user = await _users.FindByIdAsync( userId );
        if (user is null)
            return IReply.UserNotFound();

        user.UserName = update.Username;
        user.Email = update.Email;
        user.PhoneNumber = update.Phone;
        
        IdentityResult updateResult = await _users.UpdateAsync( user );
        return updateResult.Succeeds()
            ? IReply.Success()
            : IReply.ChangesNotSaved();
    }
}