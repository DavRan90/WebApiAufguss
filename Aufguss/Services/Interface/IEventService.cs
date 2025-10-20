namespace Aufguss.Services.Interface
{
    public interface IEventService
    {
        Task<List<Event>> GetEventsAsync();
        Task<Event?> GetEventByIdAsync(int id);
    }
}
