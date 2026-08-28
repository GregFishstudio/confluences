using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Confluences.Persistence;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class FieldChangeHistoriesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public FieldChangeHistoriesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/FieldChangeHistories?entityType=ApplicationUser&entityId=abc&fieldName=AssistantSocialEmail
        [HttpGet]
        public async Task<IActionResult> GetHistory(
            [FromQuery] string entityType,
            [FromQuery] string entityId,
            [FromQuery] string fieldName)
        {
            var history = await _context.FieldChangeHistories
                .AsNoTracking()
                .Include(h => h.ChangedByUser)
                .Where(h => h.EntityType == entityType && h.EntityId == entityId && h.FieldName == fieldName)
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new
                {
                    h.FieldChangeHistoryId,
                    h.OldValue,
                    h.NewValue,
                    h.ChangedAt,
                    ChangedByName = h.ChangedByUser != null
                        ? h.ChangedByUser.Firstname + " " + h.ChangedByUser.LastName
                        : null,
                })
                .ToListAsync();

            return Ok(history);
        }
    }
}
