namespace Aufguss.Services
{
    public interface IFaqService
    {
        Task UpdateFaqOrderAsync(List<Models.Faq> reorderedFaqs);
    }
}
