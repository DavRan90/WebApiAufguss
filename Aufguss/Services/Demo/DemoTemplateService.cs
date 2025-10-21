using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoTemplateService : ITemplateService
    {
        private readonly List<EventTemplate> demoTemplates = new()
    {
        new EventTemplate {
                Title = "Aufguss herrar",
                Id = 1,
                Start = DateTime.Today.AddDays(1).AddHours(19).AddMinutes(30),
                End = DateTime.Today.AddDays(1).AddHours(20).AddMinutes(15),
                MaxSlots = 5,
                Hidden = false,
                Gender = Gender.Male,
                Description = $"Aufguss pass 1"
            },
            new EventTemplate {
                Title = "Aufguss damer",
                Id = 2,
                Start = DateTime.Today.AddDays(2).AddHours(19).AddMinutes(30),
                End = DateTime.Today.AddDays(2).AddHours(20).AddMinutes(15),
                MaxSlots = 5,
                Hidden = false,
                Gender = Gender.Female,
                Description = $"Aufguss pass 2"
            },
        // Add more mock data as needed
    };

        public Task<List<EventTemplate>> GetTemplatesAsync()
        {
            return Task.FromResult(demoTemplates);
        }

        public Task<EventTemplate?> GetTemplateByIdAsync(int id)
        {
            var ev = demoTemplates.SingleOrDefault(e => e.Id == id);
            return Task.FromResult(ev);
        }
    }
}
