using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Confluences.Persistence;
using Confluences.Domain.Entities;

namespace Api.Controllers
{
    // Régions de marrainage, éditables depuis le tableau de marrainage.
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class RegionsController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public RegionsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/Regions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Region>>> GetRegions()
        {
            return await _context.Regions.AsNoTracking().OrderBy(r => r.Ordre).ThenBy(r => r.Nom).ToListAsync();
        }

        // POST: api/Regions
        [HttpPost]
        public async Task<ActionResult<Region>> PostRegion(RegionDto dto)
        {
            var nom = (dto.Nom ?? "").Trim();
            if (string.IsNullOrEmpty(nom)) return BadRequest("Le nom est requis.");
            if (await _context.Regions.AnyAsync(r => r.Nom.ToLower() == nom.ToLower()))
                return Conflict("Cette région existe déjà.");

            var region = new Region
            {
                Nom = nom,
                Couleur = dto.Couleur,
                PistesRecherche = dto.PistesRecherche,
                Ordre = dto.Ordre ?? ((await _context.Regions.MaxAsync(r => (int?)r.Ordre) ?? 0) + 1),
            };
            _context.Regions.Add(region);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRegions), new { }, region);
        }

        // PUT: api/Regions/5 — un renommage est répercuté sur les fiches qui portent l'ancien nom.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegion(int id, RegionDto dto)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region == null) return NotFound();

            var nom = (dto.Nom ?? "").Trim();
            if (string.IsNullOrEmpty(nom)) return BadRequest("Le nom est requis.");
            if (await _context.Regions.AnyAsync(r => r.RegionId != id && r.Nom.ToLower() == nom.ToLower()))
                return Conflict("Cette région existe déjà.");

            var ancien = region.Nom;
            if (ancien != nom)
            {
                foreach (var m in await _context.Marrainages.Where(m => m.Region == ancien).ToListAsync()) m.Region = nom;
                foreach (var u in await _context.Users.Where(u => u.Region == ancien).ToListAsync()) u.Region = nom;
            }

            region.Nom = nom;
            region.Couleur = dto.Couleur;
            region.PistesRecherche = dto.PistesRecherche;
            if (dto.Ordre.HasValue) region.Ordre = dto.Ordre.Value;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Regions/5 — les fiches rattachées repassent sans région (déduite de l'adresse).
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegion(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region == null) return NotFound();

            foreach (var m in await _context.Marrainages.Where(m => m.Region == region.Nom).ToListAsync()) m.Region = null;
            foreach (var u in await _context.Users.Where(u => u.Region == region.Nom).ToListAsync()) u.Region = null;
            _context.Regions.Remove(region);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class RegionDto
    {
        public string? Nom { get; set; }
        public string? Couleur { get; set; }
        public int? Ordre { get; set; }
        public string? PistesRecherche { get; set; }
    }
}
