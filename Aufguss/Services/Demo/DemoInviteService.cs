using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoInviteService : IInviteService
    {
        public Task<List<InviteDto>> GetInvitesAsync(string url)
        {
            return Task.FromResult(new List<InviteDto>
        {
            new InviteDto
    {
        Id = 1,
        InviterId = "demo-user-id",
        InviterName = "Demo user",
        InviteeId = "friend-user-id",
        InviteeName = "Friend User",
        InvitedAt = DateTime.UtcNow,
        Invited = true,
        Accepted = false
    },
            new InviteDto
    {
        Id = 2,
        InviterId = "other-user",
        InviterName = "Demo user",
        InviteeId = "demo-user-id",
        InviteeName = "Other user",
        InvitedAt = DateTime.UtcNow,
        Invited = true,
        Accepted = false
    },
        });
        }
    }
}
