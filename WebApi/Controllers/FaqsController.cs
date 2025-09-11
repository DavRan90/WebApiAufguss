using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaqsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FaqsController(ApplicationDbContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public IActionResult GetFaqs()
        {
            var faqs = _context.Faqs.OrderBy(f => f.Position).ToList();
            return Ok(faqs);
        }

        [HttpGet("{id}")]
        public IActionResult GetFaq(int id)
        {
            var faq = _context.Faqs.Find(id);
            if (faq == null)
            {
                return NotFound();
            }

            return Ok(faq);
        }

        [HttpPost]
        public IActionResult CreateFaq(FaqDto faqDto)
        {
            var count = _context.Faqs.Count();
            var faq = new Faq
            {
                Question = faqDto.Question,
                Answer = faqDto.Answer,
                Position = count,
                ShowQuestion = faqDto.ShowQuestion,
                AddedAt = DateTime.Now
            };

            _context.Faqs.Add(faq);
            _context.SaveChanges();

            return Ok(faq);
        }

        [HttpPost("update-order")]
        public async Task<IActionResult> UpdateFaqOrder([FromBody] List<FaqOrderUpdateDto> faqs)
        {
            foreach (var item in faqs)
            {
                var faq = await _context.Faqs.FindAsync(item.Id);
                if (faq != null)
                {
                    faq.Position = item.Position;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult EditFaq(int id, FaqDto faqDto)
        {
            var faq = _context.Faqs.Find(id);
            if (faq == null)
            {
                return NotFound();
            }

            faq.Question = faqDto.Question;
            faq.Answer = faqDto.Answer;
            faq.Position = faqDto.Position;
            faq.ShowQuestion = faqDto.ShowQuestion;

            _context.SaveChanges();

            return Ok(faq);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFaq(int id)
        {
            var faqToBeRemoved = _context.Faqs.Find(id);
            if (faqToBeRemoved == null)
            {
                return NotFound();
            }

            var faqs = _context.Faqs.Where(f => f.Position > faqToBeRemoved.Position).ToList();
            foreach(var faq in faqs)
            {
                faq.Position--;
            }

            _context.Faqs.Remove(faqToBeRemoved);
            _context.SaveChanges();

            return Ok();
        }
    }
}
