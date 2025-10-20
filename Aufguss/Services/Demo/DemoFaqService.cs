using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoFaqService : IFaqService
    {
        public Task<List<Faq>> GetFaqsAsync()
        {
            return Task.FromResult(new List<Faq>
        {
            new Faq {
                Id = 1,
                Question = $"Fråga 1",
                Answer = $"Svar på fråga 1"
            },
            new Faq {
                Id = 3,
                Question = $"Fråga 3",
                Answer = $"Svar på fråga 3"
            },
            new Faq {
                Id = 2,
                Question = $"Fråga 2",
                Answer = $"Svar på fråga 2"
            }
        });
        }
        public Task UpdateFaqOrderAsync(List<Faq> reorderedFaqs)
        {
            // No operation in demo mode
            return Task.CompletedTask;
        }
    }
}
