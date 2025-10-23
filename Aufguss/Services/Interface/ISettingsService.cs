using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface ISettingsService
    {
        Task<SiteSettings> GetSettingsAsync();
        Task EditSettingsAsync(SiteSettings settingsDto);
    }
}
