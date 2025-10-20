using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoEntryService : IEntryService
    {
        public Task<List<Entry>> GetEntriesAsync()
        {
            return Task.FromResult(new List<Entry>
        {
            new Entry {
                Title = "Nyhet",
                Description = "Beskrivning av nyheten...",
                CreatedAt = DateTime.Now,
                Image = "images/strand.jpg"
            },
            new Entry {
                Title = "Nyhet 2",
                Description = "Beskrivning av en annan nyhet...",
                CreatedAt = DateTime.Now,
                Image = "images/strand.jpg"
            }
        });
        }
    }
}
