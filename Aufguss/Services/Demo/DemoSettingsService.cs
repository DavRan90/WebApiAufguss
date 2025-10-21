using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoSettingsService : ISettingsService
    {
        public Task<SiteSettings> GetSettingsAsync()
        {
            return Task.FromResult(new SiteSettings {

                Id = 1,
                AufgussHiddenDaysInAdvance = 7,
                MaxBookingsAufguss = 2,
                VattenfysHiddenDaysInAdvance = 14,
                MaxBookingsVattenfys = 5,
                MaxNews = 5,
            
        });
        }
    }
}
