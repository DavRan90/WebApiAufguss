namespace Aufguss.Services.Interface
{
    public interface IEntryService
    {
        Task<List<Entry>> AddEntryAsync(EntryDto entryDto);
        Task RemoveEntryAsync(int id);
        Task<List<Entry>> GetEntriesAsync();
    }
}
