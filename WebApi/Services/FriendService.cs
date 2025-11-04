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

        /// <summary>
        /// Removes all invites and friendship connections for a given user ID.
        /// </summary>
        public async Task<bool> RemoveAllFriendRelationsForUserAsync(string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Remove invites where the user was either inviter or invitee
                var invites = await _context.Invites
                    .Where(i => i.InviterId == userId || i.InviteeId == userId)
                    .ToListAsync();

                if (invites.Any())
                    _context.Invites.RemoveRange(invites);

                // 2️⃣ Break any friendships pointing to this user
                var friends = await _context.Users
                    .Where(u => u.AufgussFriendId == userId)
                    .ToListAsync();

                foreach (var f in friends)
                    f.AufgussFriendId = null;

                // 3️⃣ If the user themselves has a friend, clear that too
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (currentUser != null && currentUser.AufgussFriendId != null)
                {
                    var friend = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == currentUser.AufgussFriendId);
                    if (friend != null)
                        friend.AufgussFriendId = null;

                    currentUser.AufgussFriendId = null;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Existing method — still useful if you need to remove a single invite elsewhere.
        /// </summary>
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
