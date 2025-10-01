using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models.Survey;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveysController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public SurveysController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSurvey(int id)
        {
            var survey = await context.Surveys
                .Include(s => s.Questions.OrderBy(q => q.Order))
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null) return NotFound();

            return Ok(survey);
        }

        [HttpPost("{surveyId}/questions")]
        public async Task<IActionResult> CreateQuestion(int surveyId, CreateSurveyQuestionDto dto)
        {
            if (surveyId != dto.SurveyId)
            {
                return BadRequest("Survey ID mismatch.");
            }

            var survey = await context.Surveys.FindAsync(surveyId);
            if (survey == null) return NotFound("Survey not found.");

            var question = new SurveyQuestion
            {
                SurveyId = dto.SurveyId,
                Text = dto.Text,
                Order = dto.Order,
                EmojiMin = dto.EmojiMin,
                EmojiMax = dto.EmojiMax
            };

            context.SurveyQuestions.Add(question);
            await context.SaveChangesAsync();

            return Ok(question);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyDto dto)
        {
            var survey = new Survey
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedAt = DateTime.Now
            };

            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            return Ok(survey);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitSurvey(SurveySubmissionDto submission)
        {
            var answer = new SurveyAnswer
            {
                SurveyId = submission.SurveyId,
                RespondentId = submission.UserId ?? Guid.NewGuid().ToString(),
                SubmittedAt = DateTime.Now,
                Details = submission.Answers.Select(a => new SurveyAnswerDetail
                {
                    QuestionId = a.QuestionId,
                    Answer = a.Answer
                }).ToList()
            };

            context.SurveyAnswers.Add(answer);
            await context.SaveChangesAsync();

            return Ok();
        }
    }
}
