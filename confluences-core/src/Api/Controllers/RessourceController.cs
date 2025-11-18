using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RessourceController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public RessourceController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/Ressource
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HomeworkV2>>> GetHomeworkV2s()
        {
            return await _context.HomeworkV2s.AsNoTracking()
                                         .Include(h => h.HomeworkType)
                                         .Include(h => h.Session)
                                            .ThenInclude(h => h.SchoolClassRoom)
                                         .Include(h => h.Teacher)
                                         .Include(h => h.Theories)
                                            .ThenInclude(h => h.Exercices)
                                         .Include(h => h.ExercicesAlones)
                                         .OrderByDescending(h => h.HomeworkV2Date)
                                         .ToListAsync();
        }
    }
}
