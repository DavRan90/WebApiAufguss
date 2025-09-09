using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaqsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public FaqsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IActionResult GetFaqs()
        {
            var faqs = context.Faqs.OrderBy(f => f.Position).ToList();
            return Ok(faqs);
        }

        [HttpGet("{id}")]
        public IActionResult GetFaq(int id)
        {
            var faq = context.Faqs.Find(id);
            if (faq == null)
            {
                return NotFound();
            }

            return Ok(faq);
        }

        [HttpPost]
        public IActionResult CreateFaq(FaqDto faqDto)
        {
            var count = context.Faqs.Count();
            var faq = new Faq
            {
                Question = faqDto.Question,
                Answer = faqDto.Answer,
                Position = count,
                ShowQuestion = faqDto.ShowQuestion,
                AddedAt = DateTime.Now
            };

            context.Faqs.Add(faq);
            context.SaveChanges();

            return Ok(faq);
        }

        [HttpPut("{id}")]
        public IActionResult EditFaq(int id, FaqDto faqDto)
        {
            var faq = context.Faqs.Find(id);
            if (faq == null)
            {
                return NotFound();
            }

            faq.Question = faqDto.Question;
            faq.Answer = faqDto.Answer;
            faq.Position = faqDto.Position;
            faq.ShowQuestion = faqDto.ShowQuestion;

            context.SaveChanges();

            return Ok(faq);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFaq(int id)
        {
            var faq = context.Faqs.Find(id);
            if (faq == null)
            {
                return NotFound();
            }

            context.Faqs.Remove(faq);
            context.SaveChanges();

            return Ok();
        }
    }
}
