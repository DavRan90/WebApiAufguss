using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface ITemplateService
    {
        Task<List<EventTemplate>> GetTemplatesAsync();
        Task<EventTemplate?> GetTemplateByIdAsync(int id);
    }
}
