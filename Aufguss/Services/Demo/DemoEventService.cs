using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoEventService : IEventService
    {
        private readonly List<Event> _events = new()
    {
        new Event {
                Title = "Aufguss herrar",
                Id = 1,
                Start = DateTime.Today.AddDays(1).AddHours(19).AddMinutes(30),
                End = DateTime.Today.AddDays(1).AddHours(20).AddMinutes(15),
                MaxSlots = 5,
                Hidden = false,
                Gender = Gender.Male,
                Description = $"Aufguss pass 1",
                AllowFriendBooking = true
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
            return Task.FromResult(_events);
        }

        public Task<List<Event>> AddEventAsync(EventDto eventDto)
        {
            var newEvent = new Event
            {
                Id = _events.Any() ? _events.Max(e => e.Id) + 1 : 1,
                Title = eventDto.Title,
                Description = eventDto.Description,
                Start = eventDto.Start,
                End = eventDto.End,
                MaxSlots = eventDto.MaxSlots,
                Hidden = eventDto.Hidden,
                Gender = eventDto.Gender,
                Recurring = eventDto.Recurring,
                IncludeSurvey = eventDto.IncludeSurvey,
                AllowFriendBooking = eventDto.AllowFriendBooking
            };

            _events.Add(newEvent);
            return Task.FromResult(_events.OrderByDescending(f => f.Id).ToList());
        }

        public Task EditEventAsync(int id, EventDto eventDto)
        {
            var existing = _events.FirstOrDefault(e => e.Id == id);
            if (existing == null)
                throw new Exception($"FAQ med ID {id} hittades inte.");

            existing.Title = eventDto.Title;
            existing.Description = eventDto.Description;
            existing.Start = eventDto.Start;
            existing.End = eventDto.End;
            existing.MaxSlots = eventDto.MaxSlots;
            existing.Hidden = eventDto.Hidden;
            existing.Gender = eventDto.Gender;
            existing.Recurring = eventDto.Recurring;
            existing.IncludeSurvey = eventDto.IncludeSurvey;
            existing.AllowFriendBooking = eventDto.AllowFriendBooking;

            return Task.CompletedTask;
        }

        public Task RemoveEventAsync(int id)
        {
            var evt = _events.SingleOrDefault(e => e.Id == id);
            if (evt != null)
            {
                _events.Remove(evt);
            }

            return Task.CompletedTask;
        }

        public Task<Event?> GetEventByIdAsync(int id)
        {
            var ev = _events.SingleOrDefault(e => e.Id == id);
            return Task.FromResult(ev);
        }
    }
}
