using Microsoft.EntityFrameworkCore;

namespace WebApi.Services
{
    public class FriendService
    {
        private readonly ApplicationDbContext _context;

        public FriendService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RemoveInviteAndBreakFriendship(int inviteId)
        {
            var invite = await _context.Invites.SingleOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null)
                return false;

            var inviter = await _context.Users.FirstOrDefaultAsync(u => u.Id == invite.InviterId);
            var invitee = await _context.Users.FirstOrDefaultAsync(u => u.Id == invite.InviteeId);

            if (inviter == null || invitee == null)
                return false;

            if (inviter.AufgussFriendId == invitee.Id)
                inviter.AufgussFriendId = null;

            if (invitee.AufgussFriendId == inviter.Id)
                invitee.AufgussFriendId = null;

            _context.Invites.Remove(invite);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
