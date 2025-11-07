using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoInviteService : IInviteService
    {
        private readonly List<InviteDto> _invites;
        private readonly IUserService _userService;
        public DemoInviteService(IUserService userService)
        {
            _userService = userService;

            _invites = new List<InviteDto>
            {
                new InviteDto
                {
                    Id = 1,
                    InviterId = "2b4ec2a0-0eaf-4a16-9412-ebd982c8c098",
                    InviterName = "Björn Johansson",
                    InviteeId = "demo-user-id",
                    InviteeName = "Demo User",
                    Invited = true,
                    Accepted = false,
                    InvitedAt = DateTime.UtcNow
                },
                new InviteDto
                {
                    Id = 2,
                    InviterId = "friend-user-id",
                    InviterName = "Friend Person",
                    InviteeId = "demo-user-id",
                    InviteeName = "Demo User",
                    Invited = true,
                    Accepted = false,
                    InvitedAt = DateTime.UtcNow
                }
            };
        }
        public Task<List<InviteDto>> GetInvitesAsync()
        {
            return Task.FromResult(_invites);
        }

        public Task<List<InviteDto>> GetInviterAsync(string id)
        {
            var inviters = _invites.Where(i => i.InviterId == id).ToList();
            return Task.FromResult(inviters);
        }
        public Task<List<InviteDto>> GetInviteeAsync(string id)
        {
            var invitees = _invites.Where(i => i.InviteeId == id).ToList();
            return Task.FromResult(invitees);
        }
        public async Task SendInviteByPhoneAsync(string phoneNumber)
        {
            // Normalize Swedish phone numbers (reuse the same logic as frontend)
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new HttpRequestException("Phone number is required.", null, System.Net.HttpStatusCode.BadRequest);

            // Simulate fetching current user (in your real app, this is the logged-in user)
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new HttpRequestException("Current user not found.", null, System.Net.HttpStatusCode.Unauthorized);

            // Find invitee by phone number
            var allUsers = await _userService.GetUsersAsync();
            var invitee = allUsers.FirstOrDefault(u => u.PhoneNumber == phoneNumber);

            if (invitee == null)
                throw new HttpRequestException("Ingen användare hittades med detta telefonnummer.",
                    null, System.Net.HttpStatusCode.NotFound);

            if (invitee.Id == currentUser.Id)
                throw new HttpRequestException("Du kan inte bjuda in dig själv.",
                    null, System.Net.HttpStatusCode.BadRequest);

            // Gender validation (if used)
            if (currentUser.Gender != null && invitee.Gender != null && currentUser.Gender != invitee.Gender)
                throw new HttpRequestException("Du kan bara bjuda in användare av samma kön.",
                    null, System.Net.HttpStatusCode.BadRequest);

            // Prevent duplicate invite
            var existingInvite = _invites.FirstOrDefault(i =>
                i.InviterId == currentUser.Id &&
                i.InviteeId == invitee.Id &&
                !i.Accepted);

            if (existingInvite != null)
                throw new HttpRequestException("En inbjudan har redan skickats till denna användare.",
                    null, System.Net.HttpStatusCode.BadRequest);

            // Create the invite
            var newInvite = new InviteDto
            {
                Id = _invites.Any() ? _invites.Max(i => i.Id) + 1 : 1,
                InviterId = currentUser.Id,
                InviterName = $"{currentUser.FirstName} {currentUser.SurName}",
                InviteeId = invitee.Id,
                InviteeName = $"{invitee.FirstName} {invitee.SurName}",
                InvitedAt = DateTime.UtcNow,
                Invited = true,
                Accepted = false
            };

            _invites.Add(newInvite);

            await Task.CompletedTask;
        }

        public Task<List<InviteDto>> AddInviteAsync(InviteDto inviteDto)
        {
            var newInvite = new InviteDto
            {
                Id = _invites.Any() ? _invites.Max(i => i.Id) + 1 : 1,
                InviterId = inviteDto.InviterId,
                InviterName = inviteDto.InviterName,
                InviteeId = inviteDto.InviteeId,
                InviteeName = inviteDto.InviteeName,
                InvitedAt = DateTime.UtcNow,
                Invited = true,
                Accepted = false
            };

            _invites.Add(newInvite);
            return Task.FromResult(_invites.OrderByDescending(i => i.Id).ToList());
        }
        public async Task RemoveInviteAsync(int id)
        {
            var invite = _invites.SingleOrDefault(i => i.Id == id);
            if (invite == null)
                throw new HttpRequestException("Invite not found", null, System.Net.HttpStatusCode.NotFound);

            var users = await _userService.GetUsersAsync();
            var inviter = users.FirstOrDefault(u => u.Id == invite.InviterId);
            var invitee = users.FirstOrDefault(u => u.Id == invite.InviteeId);

            if (inviter == null || invitee == null)
                throw new HttpRequestException("One or both users not found.", null, System.Net.HttpStatusCode.BadRequest);

            if (inviter.AufgussFriendId == invitee.Id)
                inviter.AufgussFriendId = null;

            if (invitee.AufgussFriendId == inviter.Id)
                invitee.AufgussFriendId = null;

            _invites.Remove(invite);

            await Task.CompletedTask;
        }

        public async Task<string> AcceptInviteAsync(int id, InviteDto dto)
        {
            var invite = _invites.FirstOrDefault(i => i.Id == id)
                         ?? throw new InvalidOperationException("Invite not found.");

            var allUsers = await _userService.GetUsersAsync();

            var inviter = allUsers.FirstOrDefault(u => u.Id == invite.InviterId)
                          ?? throw new InvalidOperationException("Inviter not found.");

            var invitee = allUsers.FirstOrDefault(u => u.Id == invite.InviteeId)
                           ?? throw new InvalidOperationException("Invitee not found.");

            ClearFriendship(invitee, allUsers);
            ClearFriendship(inviter, allUsers);

            // Accept the invite
            invite.Accepted = true;
            invite.Invited = false;
            invite.AcceptedAt = DateTime.UtcNow;

            // Set new friendship
            inviter.AufgussFriendId = invitee.Id;
            invitee.AufgussFriendId = inviter.Id;

            inviter.AufgussFriendName = $"{invitee.FirstName} {invitee.SurName}";
            invitee.AufgussFriendName = $"{inviter.FirstName} {inviter.SurName}";

            // Persist changes
            await Task.WhenAll(
                _userService.EditUserAsync(inviter.Id, inviter),
                _userService.EditUserAsync(invitee.Id, invitee)
            );

            // Remove other invites involving either user
            var relatedUserIds = new[] { invite.InviterId, invite.InviteeId };
            _invites.RemoveAll(i => i.Id != invite.Id &&
                                    (relatedUserIds.Contains(i.InviterId) || relatedUserIds.Contains(i.InviteeId)));

            return "Friendship accepted and updated (demo).";
        }

        private void ClearFriendship(UserWithRolesDto user, List<UserWithRolesDto> allUsers)
        {
            if (string.IsNullOrEmpty(user.AufgussFriendId))
                return;

            var oldFriend = allUsers.FirstOrDefault(u => u.Id == user.AufgussFriendId);
            if (oldFriend != null)
                oldFriend.AufgussFriendId = null;

            user.AufgussFriendId = null;

            _invites.RemoveAll(i =>
                (i.InviterId == user.Id && i.InviteeId == oldFriend?.Id) ||
                (i.InviterId == oldFriend?.Id && i.InviteeId == user.Id));
        }
    }
}

