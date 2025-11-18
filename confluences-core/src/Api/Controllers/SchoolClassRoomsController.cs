using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Confluences.Domain.Entities;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolClassRoomsController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public SchoolClassRoomsController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/SchoolClassRooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SchoolClassRoom>>> GetSchoolClassRooms()
        {
            return await _context.SchoolClassRooms.AsNoTracking().Where(s => !s.IsArchived).ToListAsync();
        }

        // GET: api/SchoolClassRooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SchoolClassRoom>> GetSchoolClassRoom(int id)
        {
            var schoolClassRoom = await _context.SchoolClassRooms
                                            .Where(s => s.SchoolClassRoomId == id)
                                            .SingleOrDefaultAsync();

            if (schoolClassRoom == null)
            {
                return NotFound();
            }

            var sessions = await _context.Sessions
                                .Include(s => s.SessionStudents).ThenInclude(s => s.Student)
                                .Include(s => s.SessionTeachers).ThenInclude(s => s.Teacher)
                                .Where(s => s.SchoolClassRoomId == id)
                                .OrderByDescending(s => s.DateStart)
                                .Take(1)
                                .ToListAsync();

            schoolClassRoom.Sessions = sessions;

            return schoolClassRoom;
        }

        [Authorize(Policy = "Teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchoolClassRoom(int id, SchoolClassRoom schoolClassRoom)
        {
            if (id != schoolClassRoom.SchoolClassRoomId)
                return BadRequest();

            _context.Entry(schoolClassRoom).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchoolClassRoomExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [Authorize(Policy = "Teacher")]
        [HttpPost]
        public async Task<ActionResult<SchoolClassRoom>> PostSchoolClassRoom(SchoolClassRoom schoolClassRoom)
        {
            _context.SchoolClassRooms.Add(schoolClassRoom);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSchoolClassRoom", new { id = schoolClassRoom.SchoolClassRoomId }, schoolClassRoom);
        }

        [Authorize(Policy = "Teacher")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<SchoolClassRoom>> DeleteSchoolClassRoom(int id)
        {
            var schoolClassRoom = await _context.SchoolClassRooms.FindAsync(id);
            if (schoolClassRoom == null)
                return NotFound();

            _context.SchoolClassRooms.Remove(schoolClassRoom);
            await _context.SaveChangesAsync();

            return schoolClassRoom;
        }

        private bool SchoolClassRoomExists(int id)
        {
            return _context.SchoolClassRooms.Any(e => e.SchoolClassRoomId == id);
        }

        // GET: api/SchoolClassRooms/aidereset/5
        [Authorize(Policy = "Teacher")]
        [HttpGet("aidereset/{id}")]
        public async Task<ActionResult> Getaidereset(int id)
        {
            var schoolClassRoom = await _context.SchoolClassRooms
                                    .Include(s => s.Sessions)
                                        .ThenInclude(s => s.SessionStudents)
                                    .Where(s => s.SchoolClassRoomId == id)
                                    .SingleOrDefaultAsync();

            if (schoolClassRoom == null)
                return NotFound();

            foreach (var session in schoolClassRoom.Sessions)
            {
                foreach (var student in session.SessionStudents)
                {
                    var user = await _context.Users
                                  .Where(s => s.Id == student.StudentId)
                                  .SingleOrDefaultAsync();

                    if (user != null)
                    {
                        user.HasSeenHelpVideo = false;
                        _context.Entry(user).State = EntityState.Modified;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/SchoolClassRooms/archive/5
        [Authorize(Policy = "Teacher")]
        [HttpPost("archive/{id}")]
        public async Task<ActionResult> PostArchived(int id)
        {
            var schoolClassRoom = await _context.SchoolClassRooms
                                    .Where(s => s.SchoolClassRoomId == id)
                                    .SingleOrDefaultAsync();

            if (schoolClassRoom == null)
                return NotFound();

            schoolClassRoom.IsArchived = true;
            _context.Entry(schoolClassRoom).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
