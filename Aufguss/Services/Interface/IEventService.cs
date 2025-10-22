namespace Aufguss.Services.Interface
{
    public interface IEventService
    {
        Task<List<Event>> GetEventsAsync();
        Task<Event?> GetEventByIdAsync(int id);
        Task<List<Event>> AddEventAsync(EventDto EventDtoData);
        Task EditEventAsync(int id, EventDto eventDto);
        Task RemoveEventAsync(int id);
    }
}
