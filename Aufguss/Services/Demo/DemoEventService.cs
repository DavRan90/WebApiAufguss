using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoEventService : IEventService
    {
        private readonly List<Event> demoEvents = new()
    {
        new Event {
                Title = "Aufguss herrar",
                Id = 1,
                Start = DateTime.Today.AddDays(1).AddHours(19).AddMinutes(30),
                End = DateTime.Today.AddDays(1).AddHours(20).AddMinutes(15),
                MaxSlots = 5,
                Hidden = false,
                Gender = Gender.Male,
                Description = $"Aufguss pass 1"
            },
            new Event {
                Title = "Aufguss damer",
                Id = 2,
                Start = DateTime.Today.AddDays(2).AddHours(19).AddMinutes(30),
                End = DateTime.Today.AddDays(2).AddHours(20).AddMinutes(15),
                MaxSlots = 5,
                Hidden = false,
                Gender = Gender.Female,
                Description = $"Aufguss pass 2"
            },
            new Event {
                Title = "Aufguss herrar",
                Id = 3,
                Start = DateTime.Today.AddDays(3).AddHours(19).AddMinutes(30),
                End = DateTime.Today.AddDays(3).AddHours(20).AddMinutes(15),
                MaxSlots = 5,
                Hidden = false,
                Gender = Gender.Male,
                Description = $"Aufguss pass 3"
            }
        // Add more mock data as needed
    };

        public Task<List<Event>> GetEventsAsync()
        {
            return Task.FromResult(demoEvents);
        }

        public Task<Event?> GetEventByIdAsync(int id)
        {
            var ev = demoEvents.SingleOrDefault(e => e.Id == id);
            return Task.FromResult(ev);
        }
    }
}
