using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface IInviteService
    {
        Task<List<InviteDto>> GetInvitesAsync(string url);
    }
}
