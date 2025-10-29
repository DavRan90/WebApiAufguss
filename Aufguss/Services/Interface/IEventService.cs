namespace Aufguss.Services.Interface
{
    public interface IEventService
    {
        Task<List<Event>> GetEventsAsync();
        Task<Event?> GetEventByIdAsync(int id);
        Task<Event> AddEventAsync(EventDto eventDto);
        Task EditEventAsync(int id, EventDto eventDto);
        Task RemoveEventAsync(int id);
    }
}
