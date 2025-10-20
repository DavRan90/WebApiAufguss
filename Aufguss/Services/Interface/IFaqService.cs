using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface IFaqService
    {
        Task<List<Faq>> GetFaqsAsync();
        Task UpdateFaqOrderAsync(List<Faq> reorderedFaqs);
    }
}
