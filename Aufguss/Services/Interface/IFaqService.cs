using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface IFaqService
    {
        Task<List<Faq>> GetFaqsAsync();
        Task<Faq> GetFaqAsync(int id);
        Task<Faq> AddFaqAsync(FaqDto faqDto);
        Task EditFaqAsync(int id, FaqDto faqDto);
        Task RemoveFaqAsync(int id);
        Task UpdateFaqOrderAsync(List<Faq> reorderedFaqs);
    }
}
