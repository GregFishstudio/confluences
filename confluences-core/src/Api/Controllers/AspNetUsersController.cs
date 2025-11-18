using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.CompilerServices;
using Confluences.Domain.Entities;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUsersController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public ApplicationUsersController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/ApplicationUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetApplicationUsers()
        {
            var ApplicationUsers =  await _context.Users
                            .AsNoTracking()
                            .Include(a => a.SessionStudents)
                                .ThenInclude(a => a.Session)
                                    .ThenInclude(a => a.SchoolClassRoom)
                            .OrderBy(a => a.Firstname)
                            .ToListAsync();


            return ApplicationUsers;
        }

        // GET: api/ApplicationUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUser>> GetApplicationUser(string id)
        {
            var ApplicationUser = await _context.Users
                                        .Where(a => a.Id == id)
                                        .Include(a => a.Gender)
                                        .Include(a => a.SessionStudents)
                                            .ThenInclude(a => a.Session)
                                                .ThenInclude(a => a.SchoolClassRoom)
                                        .SingleOrDefaultAsync();

            if (ApplicationUser == null)
            {
                return NotFound();
            }

            return ApplicationUser;
        }

        // PUT: api/ApplicationUsers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplicationUser(string id, ApplicationUser ApplicationUser)
        {
            if (id != ApplicationUser.Id)
            {
                return BadRequest();
            }

            _context.Entry(ApplicationUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationUserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ApplicationUsers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApplicationUser>> DeleteApplicationUser(string id)
        {
            var ApplicationUser = await _context.Users.FindAsync(id);
            if (ApplicationUser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(ApplicationUser);
            await _context.SaveChangesAsync();

            return ApplicationUser;
        }

        private bool ApplicationUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
