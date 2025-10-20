namespace Aufguss.Services.Interface
{
    public interface IEntryService
    {
        Task<List<Entry>> GetEntriesAsync();
    }
}
