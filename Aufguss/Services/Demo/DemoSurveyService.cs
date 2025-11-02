using Aufguss.Models.Survey;
using Aufguss.Services.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Aufguss.Services.Interface.ISurveyService;

namespace Aufguss.Services.Demo
{
    public class DemoSurveyService : ISurveyService
    {
        private readonly List<SurveyResponseDto> _responses = new();

        public Task SubmitSurveyAsync(SurveyResponseDto response)
        {
            _responses.Add(response);
            return Task.CompletedTask;
        }

        public Task<SurveyStatsDto> GetAverageAsync(string eventId)
        {
            var eventResponses = _responses.Where(r => r.EventId == eventId).ToList();

            if (!eventResponses.Any())
                return Task.FromResult(new SurveyStatsDto { EventId = eventId });

            var stats = new SurveyStatsDto
            {
                EventId = eventId,
                AvgTemperature = eventResponses.Average(r => r.Temperature),
                AvgScent = eventResponses.Average(r => r.Scent),
                AvgDuration = eventResponses.Average(r => r.Duration),
                Count = eventResponses.Count
            };

            return Task.FromResult(stats);
        }
    }

}
