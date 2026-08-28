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
    public class SessionStudentsController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public SessionStudentsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/SessionStudents?year=2026&quarter=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SessionStudent>>> GetSessionStudents(
            [FromQuery] int? year,
            [FromQuery] int? quarter)
        {
            var startOfYear = year.HasValue ? new DateTime(year.Value, 1, 1) : (DateTime?)null;
            var endOfYear   = year.HasValue ? new DateTime(year.Value, 12, 31) : (DateTime?)null;

            return await _context.SessionStudents
                .AsNoTracking()
                .Include(s => s.Session)
                    .ThenInclude(s => s.SchoolClassRoom)
                .Include(s => s.Student)
                .Where(s => s.Session != null)
                .Where(s => !year.HasValue    || (s.Session.DateStart >= startOfYear && s.Session.DateStart <= endOfYear))
                .Where(s => !quarter.HasValue || s.Session.SessionNumberId == quarter.Value)
                .ToListAsync();
        }

        // GET: api/SessionStudents/5
        [HttpGet("{sessionId}/{studentId}")]
        public async Task<ActionResult<SessionStudent>> GetSessionStudent(int sessionId, string studentId)
        {
            var sessionStudent = await _context.SessionStudents
                                           .Include(s => s.Session)
                                                .ThenInclude(s => s.SchoolClassRoom)
                                           .Include(s => s.Student)
                                           .Where(s => s.SessionId == sessionId && s.StudentId == studentId)
                                           .SingleOrDefaultAsync();

            if (sessionStudent == null)
            {
                return NotFound();
            }

            return sessionStudent;
        }

        // POST: api/SessionStudents/5/abc-123/taux
        // Recalcule (et met en cache) le taux de participation pour cette session/participant-e,
        // à partir des présences enregistrées. Ne recalcule pas si déjà fait il y a moins d'une
        // heure (sauf ?force=true), pour éviter de ré-agréger les présences à chaque affichage.
        //
        // Les dates de "Session" ne correspondent pas aux vraies dates de cours (données
        // historiques incohérentes) : la vraie période vient de "SessionFormation" (le
        // calendrier utilisé par le registre de présence). On reproduit ici exactement la
        // logique de correspondance d'AttendanceRegister.vue (findSessionLink/matchesGroup) :
        // le type de classe (télécours/F&I A1/F&I A2) déduit du nom de la classe, puis la
        // Nième SessionFormation de ce type (toutes années confondues, triées par date de
        // début), où N = SessionNumberId — seulement si l'année de cette Nième entrée
        // correspond bien à l'année de la Session. Si aucune correspondance fiable n'est
        // trouvée, on renvoie un taux nul plutôt qu'un chiffre potentiellement faux.
        [HttpPost("{sessionId}/{studentId}/taux")]
        public async Task<ActionResult<SessionStudent>> RecalculerTaux(int sessionId, string studentId, [FromQuery] bool force = false)
        {
            var sessionStudent = await _context.SessionStudents
                .Include(s => s.Session)
                    .ThenInclude(s => s!.SchoolClassRoom)
                .SingleOrDefaultAsync(s => s.SessionId == sessionId && s.StudentId == studentId);

            if (sessionStudent == null || sessionStudent.Session == null) return NotFound();

            var isFresh = sessionStudent.TauxParticipationCalculeLe.HasValue
                && (DateTime.UtcNow - sessionStudent.TauxParticipationCalculeLe.Value) < TimeSpan.FromHours(1);

            if (!force && isFresh) return sessionStudent;

            var period = await ResolveSessionFormationPeriod(sessionStudent.Session);

            if (period == null)
            {
                sessionStudent.TauxParticipation = null;
                sessionStudent.TauxParticipationCalculeLe = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return sessionStudent;
            }

            var presences = await _context.Presences
                .Where(p => p.StagiaireId == studentId
                    && p.Date >= period.Value.Start
                    && p.Date <= period.Value.End)
                .Select(p => p.Statut)
                .ToListAsync();

            // p/r comptent comme présence (retard reste une présence), a/e comme absence,
            // s (stage) est hors mesure de la participation en classe.
            var present = presences.Count(s => s == "p" || s == "r");
            var absent = presences.Count(s => s == "a" || s == "e");
            var total = present + absent;

            sessionStudent.TauxParticipation = total > 0 ? Math.Round(100.0 * present / total, 1) : (double?)null;
            sessionStudent.TauxParticipationCalculeLe = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return sessionStudent;
        }

        private async Task<(DateTime Start, DateTime End)?> ResolveSessionFormationPeriod(Session session)
        {
            var className = session.SchoolClassRoom?.SchoolClassRoomName?.ToLower() ?? "";
            string type;
            if (className.Contains("tele") || className.Contains("télé")) type = "telecours";
            else if (className.Contains("a1")) type = "fi_a1";
            else if (className.Contains("a2")) type = "fi_a2";
            else return null;

            var quarter = session.SessionNumberId;
            if (quarter <= 0) return null;

            var sameTypeOrdered = await _context.SessionFormations
                .Where(sf => sf.Type.ToLower() == type)
                .OrderBy(sf => sf.StartDate)
                .ToListAsync();

            var index = quarter - 1;
            if (index < 0 || index >= sameTypeOrdered.Count) return null;

            var candidate = sameTypeOrdered[index];
            if (candidate.StartDate.Year != session.DateStart.Year) return null;

            return (candidate.StartDate, candidate.EndDate);
        }

        // POST: api/SessionStudents
        [HttpPost]
        public async Task<ActionResult<SessionStudent>> PostSessionStudent(SessionStudent sessionStudent)
        {
            _context.SessionStudents.Add(sessionStudent);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SessionStudentExists(sessionStudent.SessionId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSessionStudent", new { sessionId = sessionStudent.SessionId, studentId = sessionStudent.StudentId }, sessionStudent);
        }

        // DELETE: api/SessionStudents/5
        [HttpDelete("{sessionId}/{studentId}")]
        public async Task<ActionResult<SessionStudent>> DeleteSessionStudent(int sessionId, string studentId)
        {
            var sessionStudent = await _context.SessionStudents
                                            .Where(s => s.SessionId == sessionId && s.StudentId == studentId)
                                            .SingleOrDefaultAsync();
            if (sessionStudent == null)
            {
                return NotFound();
            }

            _context.SessionStudents.Remove(sessionStudent);
            await _context.SaveChangesAsync();

            return sessionStudent;
        }

        private bool SessionStudentExists(int id)
        {
            return _context.SessionStudents.Any(e => e.SessionId == id);
        }
    }
}