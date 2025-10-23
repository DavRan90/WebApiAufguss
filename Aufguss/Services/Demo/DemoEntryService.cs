using Aufguss.Services.Interface;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.Demo
{
    public class DemoEntryService : IEntryService
    {
        private readonly List<Entry> _entries = new()
    {
        new Entry {
            Id = 1,
                Title = "Nyhet",
                Description = "Beskrivning av nyheten...",
                CreatedAt = DateTime.Now,
                Image = "images/strand.jpg"
            },
            new Entry {
                Id = 2,
                Title = "Nyhet 2",
                Description = "Beskrivning av en annan nyhet...",
                CreatedAt = DateTime.Now,
                Image = "images/strand.jpg"
            }
    };
        public Task<List<Entry>> GetEntriesAsync()
        {
            return Task.FromResult(_entries.OrderByDescending(f => f.Id).ToList());
        }

        public Task<List<Entry>> AddEntryAsync(EntryDto entryDto)
        {
            var newEntry = new Entry
            {
                Id = _entries.Any() ? _entries.Max(f => f.Id) + 1 : 1,
                Title = entryDto.Title,
                Description = entryDto.Description,
                CreatedAt = DateTime.Now,
                Image = entryDto.Image
            };

            _entries.Add(newEntry);
            return Task.FromResult(_entries.OrderByDescending(f => f.Id).ToList());
        }
        public Task RemoveEntryAsync(int id)
        {
            var entry = _entries.SingleOrDefault(e => e.Id == id);
            if (entry != null)
            {
                _entries.Remove(entry);
            }

            return Task.CompletedTask;
        }
    }
}
