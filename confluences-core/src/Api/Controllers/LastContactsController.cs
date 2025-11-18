using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class LastContactsController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public LastContactsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/LastContacts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LastContact>>> GetLastContacts()
        {
            return await _context.LastContacts
                                    .Include(c => c.Entreprise)
                                    .OrderBy(s => s.Entreprise.Nom)
                                    .ToListAsync();
        }

        // GET: api/LastContacts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LastContact>> GetLastContact(int id)
        {
            var LastContact = await _context.LastContacts
                                        .Include(c => c.Entreprise)
                                        .Where(c => c.LastContactId == id)
                                        .SingleOrDefaultAsync();

            if (LastContact == null)
            {
                return NotFound();
            }

            return LastContact;
        }

        // PUT: api/LastContacts/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLastContact(int id, LastContact LastContact)
        {
            if (id != LastContact.LastContactId)
            {
                return BadRequest();
            }

            _context.Entry(LastContact).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LastContactExists(id))
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

        // POST: api/LastContacts
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<LastContact>> PostLastContact(LastContact LastContact)
        {
            _context.LastContacts.Add(LastContact);
            await _context.SaveChangesAsync();

            LastContact.Entreprise = await _context.Entreprises.Where(e => e.EntrepriseId == LastContact.EntrepriseId).SingleOrDefaultAsync();

            return CreatedAtAction("GetLastContact", new { id = LastContact.LastContactId }, LastContact);
        }

        // DELETE: api/LastContacts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<LastContact>> DeleteLastContact(int id)
        {
            var LastContact = await _context.LastContacts.FindAsync(id);
            if (LastContact == null)
            {
                return NotFound();
            }

            _context.LastContacts.Remove(LastContact);
            await _context.SaveChangesAsync();

            return LastContact;
        }

        private bool LastContactExists(int id)
        {
            return _context.LastContacts.Any(e => e.LastContactId == id);
        }
    }
}
