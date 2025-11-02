using Aufguss.Models.Survey;
using System.Collections.Generic;

namespace Aufguss.Services.Interface
    
{
    public interface ISurveyService
    {
        Task SubmitSurveyAsync(SurveyResponseDto response);
        Task<SurveyStatsDto> GetAverageAsync(string eventId);
    }
}
