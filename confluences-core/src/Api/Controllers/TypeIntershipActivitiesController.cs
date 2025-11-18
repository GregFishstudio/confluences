/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 23.09.2022
 * Description : Contrôleur permettant le CRUD sur la table TypeIntershipActivities
 * Fichier : TypeIntershipActivitiesController.cs
 **/

using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class TypeIntershipActivitiesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public TypeIntershipActivitiesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/TypeIntershipActivities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeIntershipActivity>>> GetTypeIntershipActivities()
        {
            return await _context.TypeIntershipActivities.AsNoTracking().OrderBy(s => s.Nom).ToListAsync();
        }

        // GET: api/TypeIntershipActivities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeIntershipActivity>> GetTypeIntershipActivity(int id)
        {
            var TypeIntershipActivity = await _context.TypeIntershipActivities
                .Include(t => t.Stages)
                    .ThenInclude(t => t.Stagiaire)
                .Where(t => t.TypeIntershipActivityId == id)
                .SingleOrDefaultAsync();

            if (TypeIntershipActivity == null)
            {
                return NotFound();
            }

            return TypeIntershipActivity;
        }

        // PUT: api/TypeIntershipActivities/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTypeIntershipActivity(int id, TypeIntershipActivity TypeIntershipActivity)
        {
            if (id != TypeIntershipActivity.TypeIntershipActivityId)
            {
                return BadRequest();
            }

            _context.Entry(TypeIntershipActivity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeIntershipActivityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                if (TypeIntershipActivityUniqueExists(TypeIntershipActivity.Nom))
                {
                    return Conflict();
                }
            }

            return NoContent();
        }

        // POST: api/TypeIntershipActivities
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TypeIntershipActivity>> PostTypeIntershipActivity(TypeIntershipActivity TypeIntershipActivity)
        {
            _context.TypeIntershipActivities.Add(TypeIntershipActivity);

            if (TypeIntershipActivityUniqueExists(TypeIntershipActivity.Nom))
            {
                return Conflict();
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTypeIntershipActivity", new { id = TypeIntershipActivity.TypeIntershipActivityId }, TypeIntershipActivity);
        }

        // DELETE: api/TypeIntershipActivities/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TypeIntershipActivity>> DeleteTypeIntershipActivity(int id)
        {
            var TypeIntershipActivity = await _context.TypeIntershipActivities.FindAsync(id);
            if (TypeIntershipActivity == null)
            {
                return NotFound();
            }

            _context.TypeIntershipActivities.Remove(TypeIntershipActivity);
            await _context.SaveChangesAsync();

            return TypeIntershipActivity;
        }

        private bool TypeIntershipActivityExists(int id)
        {
            return _context.TypeIntershipActivities.Any(e => e.TypeIntershipActivityId == id);
        }

        private bool TypeIntershipActivityUniqueExists(string nom)
        {
            return _context.TypeIntershipActivities.Any(e => e.Nom == nom);
        }
    }
}
