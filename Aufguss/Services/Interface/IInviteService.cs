using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface IInviteService
    {
        Task<List<InviteDto>> GetInvitesAsync();
        Task<List<InviteDto>> GetInviterAsync(string id);
        Task<List<InviteDto>> GetInviteeAsync(string id);
        Task<List<InviteDto>> AddInviteAsync(InviteDto inviteDto);
        Task RemoveInviteAsync(int id);
        Task<string> AcceptInviteAsync(int id, InviteDto dto);
    }
}
