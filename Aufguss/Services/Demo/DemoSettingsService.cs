using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoSettingsService : ISettingsService
    {
        private SiteSettings _settings = new SiteSettings
        {
            Id = 1,
            AufgussHiddenDaysInAdvance = 7,
            MaxBookingsAufguss = 2,
            VattenfysHiddenDaysInAdvance = 14,
            MaxBookingsVattenfys = 5,
            MaxNews = 5
        };

        public Task<SiteSettings> GetSettingsAsync()
        {
            // Return a copy to simulate data retrieval
            var settingsCopy = new SiteSettings
            {
                Id = _settings.Id,
                AufgussHiddenDaysInAdvance = _settings.AufgussHiddenDaysInAdvance,
                MaxBookingsAufguss = _settings.MaxBookingsAufguss,
                VattenfysHiddenDaysInAdvance = _settings.VattenfysHiddenDaysInAdvance,
                MaxBookingsVattenfys = _settings.MaxBookingsVattenfys,
                MaxNews = _settings.MaxNews
            };
            return Task.FromResult(settingsCopy);
        }

        public Task EditSettingsAsync(SiteSettings settingsDto)
        {
            // Update the in-memory settings with new values
            _settings.AufgussHiddenDaysInAdvance = settingsDto.AufgussHiddenDaysInAdvance;
            _settings.MaxBookingsAufguss = settingsDto.MaxBookingsAufguss;
            _settings.VattenfysHiddenDaysInAdvance = settingsDto.VattenfysHiddenDaysInAdvance;
            _settings.MaxBookingsVattenfys = settingsDto.MaxBookingsVattenfys;
            _settings.MaxNews = settingsDto.MaxNews;

            return Task.CompletedTask;
        }
    }
}
