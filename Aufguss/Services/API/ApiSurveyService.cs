using Aufguss.Models.Survey;
using Aufguss.Services.Interface;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static Aufguss.Services.Interface.ISurveyService;

namespace Aufguss.Services.API
{
    public class ApiSurveyService : ISurveyService
    {
        private readonly HttpClient _http;

        public ApiSurveyService(HttpClient http)
        {
            _http = http;
        }

        public async Task SubmitSurveyAsync(SurveyResponseDto response)
        {
            await _http.PostAsJsonAsync($"api/surveys/{response.EventId}", response);
        }

        public async Task<SurveyStatsDto> GetAverageAsync(string eventId)
        {
            return await _http.GetFromJsonAsync<SurveyStatsDto>($"api/surveys/{eventId}/average")
                   ?? new SurveyStatsDto { EventId = eventId };
        }

    }
}
