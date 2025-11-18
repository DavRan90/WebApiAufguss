using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface ITemplateService
    {
        Task<List<EventTemplate>> GetTemplatesAsync();
        Task<EventTemplate?> GetTemplateByIdAsync(int id);
        Task<EventTemplate> CreateTemplateAsync(EventDto eventDto);
        Task DeleteTemplateAsync(int id);
    }
}
