// ─────────────────────────────────────────────────────────────────────────────
// Fichier : Api/Controllers/CalendrierController.cs
// ─────────────────────────────────────────────────────────────────────────────
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluences.Domain.Dto;
using Confluences.Domain.Entities;
using Confluences.Persistence;   // ← ajustez selon votre namespace DbContext

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalendrierController : ControllerBase
    {
        private readonly ConfluencesDbContext _db;

        public CalendrierController(ConfluencesDbContext db)
        {
            _db = db;
        }

        // ════════════════════════════════════════════════════════════════════
        // GET /api/calendrier  → tout en un seul appel (pour PresenceView)
        // ════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<ActionResult<CalendrierCompletDto>> GetAll()
        {
            var sessions = await _db.SessionFormations.OrderBy(s => s.StartDate).ToListAsync();
            var feries   = await _db.JoursFeries.OrderBy(f => f.Date).ToListAsync();
            var vacances = await _db.PeriodesVacances.OrderBy(v => v.StartDate).ToListAsync();

            return Ok(new CalendrierCompletDto
            {
                Sessions = sessions.Select(ToDto).ToList(),
                Feries   = feries.Select(f => new JourFerieDto
                    { Id = f.Id, Date = f.Date, Label = f.Label }).ToList(),
                Vacances = vacances.Select(v => new PeriodeVacanceDto
                    { Id = v.Id, Label = v.Label, StartDate = v.StartDate, EndDate = v.EndDate }).ToList(),
            });
        }

        // ════════════════════════════════════════════════════════════════════
        // SESSIONS
        // ════════════════════════════════════════════════════════════════════

        [HttpGet("sessions")]
        public async Task<ActionResult<List<SessionFormationDto>>> GetSessions()
        {
            var list = await _db.SessionFormations.OrderBy(s => s.StartDate).ToListAsync();
            return Ok(list.Select(ToDto));
        }

        /// <summary>
        /// PUT /api/calendrier/sessions
        /// Reçoit la liste complète côté client et effectue un upsert + suppression des absents.
        /// </summary>
        [HttpPut("sessions")]
        public async Task<IActionResult> UpsertSessions([FromBody] List<SessionFormationDto> dtos)
        {
            if (dtos == null) return BadRequest();

            var existing = await _db.SessionFormations.ToListAsync();
            var incomingIds = dtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToHashSet();

            // Supprimer ceux qui ne sont plus dans la liste
            var toDelete = existing.Where(e => !incomingIds.Contains(e.Id)).ToList();
            _db.SessionFormations.RemoveRange(toDelete);

            foreach (var dto in dtos)
            {
                if (dto.Id.HasValue)
                {
                    var entity = existing.FirstOrDefault(e => e.Id == dto.Id.Value);
                    if (entity != null) ApplyDto(entity, dto);
                }
                else
                {
                    var entity = new SessionFormation();
                    ApplyDto(entity, dto);
                    _db.SessionFormations.Add(entity);
                }
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("sessions/{id:int}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var entity = await _db.SessionFormations.FindAsync(id);
            if (entity == null) return NotFound();
            _db.SessionFormations.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ════════════════════════════════════════════════════════════════════
        // JOURS FÉRIÉS
        // ════════════════════════════════════════════════════════════════════

        [HttpGet("feries")]
        public async Task<ActionResult<List<JourFerieDto>>> GetFeries()
        {
            var list = await _db.JoursFeries.OrderBy(f => f.Date).ToListAsync();
            return Ok(list.Select(f => new JourFerieDto { Id = f.Id, Date = f.Date, Label = f.Label }));
        }

        /// <summary>
        /// PUT /api/calendrier/feries
        /// Reçoit la liste complète des dates ISO (string[]), toutes années confondues,
        /// et remplace entièrement la liste existante.
        /// </summary>
        [HttpPut("feries")]
        public async Task<IActionResult> UpsertFeries([FromBody] List<string> dateStrings)
        {
            if (dateStrings == null) return BadRequest();

            var dates = dateStrings
                .Select(d => DateTime.TryParse(d, out var dt) ? (DateTime?)dt.Date : null)
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .Distinct()
                .ToList();

            // Le client envoie toujours la liste complète : on supprime les absents
            // et on ajoute les nouveaux (les libellés existants sont conservés).
            var existing = await _db.JoursFeries.ToListAsync();
            var dateSet  = dates.ToHashSet();

            _db.JoursFeries.RemoveRange(existing.Where(f => !dateSet.Contains(f.Date.Date)));

            var existingDates = existing.Select(f => f.Date.Date).ToHashSet();
            foreach (var date in dates.Where(d => !existingDates.Contains(d)))
            {
                _db.JoursFeries.Add(new JourFerie { Date = date });
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("feries/{id:int}")]
        public async Task<IActionResult> DeleteFerie(int id)
        {
            var entity = await _db.JoursFeries.FindAsync(id);
            if (entity == null) return NotFound();
            _db.JoursFeries.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ════════════════════════════════════════════════════════════════════
        // VACANCES GÉNÉRALES
        // ════════════════════════════════════════════════════════════════════

        [HttpGet("vacances")]
        public async Task<ActionResult<List<PeriodeVacanceDto>>> GetVacances()
        {
            var list = await _db.PeriodesVacances.OrderBy(v => v.StartDate).ToListAsync();
            return Ok(list.Select(v => new PeriodeVacanceDto
                { Id = v.Id, Label = v.Label, StartDate = v.StartDate, EndDate = v.EndDate }));
        }

        /// <summary>
        /// PUT /api/calendrier/vacances
        /// Upsert + suppression des absents, même logique que les sessions.
        /// </summary>
        [HttpPut("vacances")]
        public async Task<IActionResult> UpsertVacances([FromBody] List<PeriodeVacanceDto> dtos)
        {
            if (dtos == null) return BadRequest();

            var existing    = await _db.PeriodesVacances.ToListAsync();
            var incomingIds = dtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToHashSet();

            var toDelete = existing.Where(e => !incomingIds.Contains(e.Id)).ToList();
            _db.PeriodesVacances.RemoveRange(toDelete);

            foreach (var dto in dtos)
            {
                if (dto.Id.HasValue)
                {
                    var entity = existing.FirstOrDefault(e => e.Id == dto.Id.Value);
                    if (entity != null)
                    {
                        entity.Label     = dto.Label;
                        entity.StartDate = dto.StartDate.Date;
                        entity.EndDate   = dto.EndDate.Date;
                        entity.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    _db.PeriodesVacances.Add(new PeriodeVacance
                    {
                        Label     = dto.Label,
                        StartDate = dto.StartDate.Date,
                        EndDate   = dto.EndDate.Date,
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("vacances/{id:int}")]
        public async Task<IActionResult> DeleteVacance(int id)
        {
            var entity = await _db.PeriodesVacances.FindAsync(id);
            if (entity == null) return NotFound();
            _db.PeriodesVacances.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers privés
        // ════════════════════════════════════════════════════════════════════

        private static SessionFormationDto ToDto(SessionFormation e) => new()
        {
            Id           = e.Id,
            Name         = e.Name,
            Type         = e.Type,
            StartDate    = e.StartDate,
            EndDate      = e.EndDate,
            VacanceStart = e.VacanceStart,
            VacanceEnd   = e.VacanceEnd,
        };

        private static void ApplyDto(SessionFormation entity, SessionFormationDto dto)
        {
            entity.Name         = dto.Name;
            entity.Type         = dto.Type;
            entity.StartDate    = dto.StartDate.Date;
            entity.EndDate      = dto.EndDate.Date;
            entity.VacanceStart = dto.VacanceStart?.Date;
            entity.VacanceEnd   = dto.VacanceEnd?.Date;
            entity.UpdatedAt    = DateTime.UtcNow;
        }
    }
}