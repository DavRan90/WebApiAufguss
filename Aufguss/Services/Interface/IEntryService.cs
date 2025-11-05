namespace Aufguss.Services.Interface
{
    public interface IEntryService
    {
        Task<Entry?> AddEntryAsync(EntryDto entryDtoData);
        Task RemoveEntryAsync(int id);
        Task<List<Entry>> GetEntriesAsync();
    }
}
