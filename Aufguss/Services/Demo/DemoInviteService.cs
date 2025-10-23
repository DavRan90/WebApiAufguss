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
            var invite = _invites.FirstOrDefault(i => i.Id == id);
            if (invite == null)
                throw new InvalidOperationException("Invite not found.");

            var allUsers = await _userService.GetUsersAsync();

            var inviter = allUsers.FirstOrDefault(u => u.Id == invite.InviterId);
            var invitee = allUsers.FirstOrDefault(u => u.Id == invite.InviteeId);

            if (inviter == null || invitee == null)
                throw new InvalidOperationException("One or both users not found.");

            if (!string.IsNullOrEmpty(invitee.AufgussFriendId))
            {
                var oldFriend = allUsers.FirstOrDefault(u => u.Id == invitee.AufgussFriendId);
                if (oldFriend != null)
                    oldFriend.AufgussFriendId = null;

                invitee.AufgussFriendId = null;

                _invites.RemoveAll(i =>
                    (i.InviterId == invitee.Id && i.InviteeId == oldFriend?.Id) ||
                    (i.InviterId == oldFriend?.Id && i.InviteeId == invitee.Id));
            }

            if (!string.IsNullOrEmpty(inviter.AufgussFriendId))
            {
                var inviterOldFriend = allUsers.FirstOrDefault(u => u.Id == inviter.AufgussFriendId);
                if (inviterOldFriend != null)
                    inviterOldFriend.AufgussFriendId = null;

                inviter.AufgussFriendId = null;

                _invites.RemoveAll(i =>
                    (i.InviterId == inviter.Id && i.InviteeId == inviterOldFriend?.Id) ||
                    (i.InviterId == inviterOldFriend?.Id && i.InviteeId == inviter.Id));
            }

            invite.Accepted = true;
            invite.Invited = false;
            invite.AcceptedAt = DateTime.UtcNow;

            inviter.AufgussFriendId = invite.InviteeId;
            invitee.AufgussFriendId = invite.InviterId;

            _invites.RemoveAll(i =>
                i.Id != invite.Id &&
                (i.InviteeId == invite.InviteeId || i.InviterId == invite.InviteeId ||
                 i.InviteeId == invite.InviterId || i.InviterId == invite.InviterId));

            return "Friendship accepted and updated (demo).";
        }
    }
}

