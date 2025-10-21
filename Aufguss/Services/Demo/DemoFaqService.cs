using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoFaqService : IFaqService
    {
        private readonly List<Faq> _faqs = new()
    {
        new Faq { Id = 1, Question = "Vad är Aufguss?", Answer = "En bastuceremoni...", Position = 1 },
        new Faq { Id = 2, Question = "Behöver jag boka?", Answer = "Ja, boka online...", Position = 2 }
    };

        public Task<List<Faq>> GetFaqsAsync()
        {
            return Task.FromResult(_faqs.OrderBy(f => f.Position).ToList());
        }

        public Task<Faq> GetFaqAsync(int id)
        {
            var faq = _faqs.FirstOrDefault(f => f.Id == id);
            return Task.FromResult(faq ?? throw new Exception($"FAQ med ID {id} hittades inte."));
        }

        public Task<Faq> AddFaqAsync(FaqDto faqDto)
        {
            var newFaq = new Faq
            {
                Id = _faqs.Any() ? _faqs.Max(f => f.Id) + 1 : 1,
                Question = faqDto.Question,
                Answer = faqDto.Answer,
                Position = _faqs.Count + 1
            };

            _faqs.Add(newFaq);
            return Task.FromResult(newFaq);
        }
        public Task EditFaqAsync(int id, FaqDto faqDto)
        {
            var existing = _faqs.FirstOrDefault(f => f.Id == id);
            if (existing == null)
                throw new Exception($"FAQ med ID {id} hittades inte.");

            existing.Question = faqDto.Question;
            existing.Answer = faqDto.Answer;

            return Task.CompletedTask;
        }

        public Task RemoveFaqAsync(int id)
        {
            var faq = _faqs.SingleOrDefault(f => f.Id == id);
            if (faq != null)
            {
                _faqs.Remove(faq);
            }

            return Task.CompletedTask;
        }

        public Task UpdateFaqOrderAsync(List<Faq> reorderedFaqs)
        {
            foreach (var updated in reorderedFaqs)
            {
                var existing = _faqs.FirstOrDefault(f => f.Id == updated.Id);
                if (existing != null)
                {
                    existing.Position = updated.Position;
                }
            }

            return Task.CompletedTask;
        }
    }
}
