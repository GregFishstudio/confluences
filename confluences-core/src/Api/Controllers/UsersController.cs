/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020 (Mis à jour 08.04.2026)
 * Description : Contrôleur permettant de lire/modifier les utilisateurs sur la table AspNetUsers
 * Fichier : UsersController.cs
 **/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluences.Persistence;
using Api.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public UsersController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
public async Task<ActionResult<IEnumerable<object>>> GetUsers()
{
    var users = await _context.Users
        .AsNoTracking()
        .Include(u => u.SessionStudents)
            .ThenInclude(ss => ss.Session)
                .ThenInclude(s => s.SchoolClassRoom)
        .Include(u => u.TypeAffiliation)
        .OrderBy(u => u.Firstname)
        .Select(u => new
        {
            id                    = u.Id,
            prenom                = u.Firstname,
            nom                   = u.LastName,
            userName              = u.UserName,
            email                 = u.Email,
            phoneNumber           = u.PhoneNumber,
            birthday              = u.Birthday,
            nationality           = u.Nationality,
            language              = u.Language,
            dateDebutParticipation = u.DateDebutParticipation,
            dateFinParticipation = u.DateFinParticipation,
            raisonInterruptionMesure = u.RaisonInterruptionMesure,
            typeAffiliation       = u.TypeAffiliation == null ? null : new
            {
                id   = u.TypeAffiliation.TypeAffiliationId,
                name = u.TypeAffiliation.Libelle
            },
            niveau = u.SessionStudents
                .OrderByDescending(ss => ss.Session.DateStart)
                .Select(ss => ss.Session.SchoolClassRoom.SchoolClassRoomName)
                .FirstOrDefault() ?? "N/A",
            currentSessionNumber = u.SessionStudents
                .OrderByDescending(ss => ss.Session.DateStart)
                .Select(ss => (int?)ss.Session.SessionNumberId)
                .FirstOrDefault(),
            currentSessionYear = u.SessionStudents
                .OrderByDescending(ss => ss.Session.DateStart)
                .Select(ss => (int?)ss.Session.DateStart.Year)
                .FirstOrDefault(),
        })
        .ToListAsync();

    return Ok(users);
}

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, [FromBody] UserUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Une interruption a toujours une date et une raison.
            if (dto.DateFinParticipation.HasValue && string.IsNullOrWhiteSpace(dto.RaisonInterruptionMesure))
                return BadRequest("La raison de l'interruption est obligatoire.");

            user.DateDebutParticipation = dto.DateDebutParticipation;
            user.DateFinParticipation = dto.DateFinParticipation;
            user.RaisonInterruptionMesure = dto.RaisonInterruptionMesure;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }
    }
}