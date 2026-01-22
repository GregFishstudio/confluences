/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Contrôleur permettant le CRUD sur la table AspNetUsers
 * Fichier : StagiairesController.cs
 **/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Api.ViewModel;
using Confluences.Domain.Entities;
using System;

namespace Api.Controllers
{
    [Authorize(Policy = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class StagiairesController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public StagiairesController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/Stagiaires
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StagiaireInfo>>> GetStagiaires()
        {
            var aspnetusers = await _context.Users
                            .AsNoTracking()
                            .Include(a => a.TypeAffiliation)
                            .Include(a => a.StageStagiaires)
                            .Include(a => a.SessionStudents) // <-- AJOUT 1: Inclure les participations aux sessions
                            .ThenInclude(ss => ss.Session) // <-- AJOUT 2: Inclure les sessions elles-mêmes
                            .ThenInclude(s => s.SchoolClassRoom)
                            .Where(a => a.StageStagiaires.Count > 0)
                            .OrderBy(a => a.Firstname)
                            .Select(v => new StagiaireInfo { 
                                StagiaireId = v.Id, 
                                Username = v.NormalizedUserName, 
                                Prenom = v.Firstname, 
                                Nom = v.LastName, 
                                OralLevel = v.OralLevel,       
                                WrittenLevel = v.WrittenLevel,
                                DateDebutParticipation = v.DateDebutParticipation,
                                TypeAffiliation = v.TypeAffiliation, 
                                StageStagiaires = v.StageStagiaires, 
                                TypeAffiliationId = v.TypeAffiliation == null ? 0 : v.TypeAffiliation.TypeAffiliationId,
                                SessionStudents = v.SessionStudents.ToList(),
                            })
                            .ToListAsync();

            return aspnetusers;
        }

        // GET: api/Stagiaires/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StagiaireInfo>> GetStagiaire(string id)
        {
            var aspNetUser = await _context.Users
                            .AsNoTracking()
                            .Include(a => a.TypeAffiliation)
                            .Include(a => a.StageStagiaires)
                            .ThenInclude(a => a.Entreprise)
                            .Include(a => a.StageStagiaires)
                            .ThenInclude(a => a.TypeMetier)
                            .Include(a => a.EntreprisFormateurIdDernierContacts)
                            .Include(a => a.EntreprisStagiaireIdDernierContactNavigations)
                            .Include(a => a.StageCreateurs)
                            .Where(a => a.StageStagiaires.Count() > 0 && a.Id == id)
                            .OrderBy(s => s.Firstname)
                            .Select(v => new StagiaireInfo { 
                                StagiaireId = v.Id, 
                                Prenom = v.Firstname, 
                                Nom = v.LastName, 
                                TypeAffiliation = v.TypeAffiliation, 
                                StageStagiaires = v.StageStagiaires,
                                StageCreateurs = v.StageCreateurs, 
                                OralLevel = v.OralLevel,       
                                WrittenLevel = v.WrittenLevel,
                                DateDebutParticipation = v.DateDebutParticipation,
                                TypeAffiliationId = v.TypeAffiliation == null ? 0 : v.TypeAffiliation.TypeAffiliationId,
                                EntreprisStagiaireIdDernierContactNavigations = v.EntreprisStagiaireIdDernierContactNavigations,
                                EntreprisFormateurIdDernierContactNavigations = v.EntreprisFormateurIdDernierContacts
                            })
                            .SingleOrDefaultAsync();

            if (aspNetUser.StageStagiaires != null)
            {
                aspNetUser.StageStagiaires = aspNetUser.StageStagiaires.OrderByDescending(s => s.Debut).ToList();
            }

            if (aspNetUser == null)
            {
                return NotFound();
            }

            return aspNetUser;
        }

        // PUT: api/Stagiaires/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStagiaire(string id, StagiaireInfo stagiaire)
        {
            if (id != stagiaire.StagiaireId)
            {
                return BadRequest();
            }

            ApplicationUser aspnetUser = _context.Users.Where(a => a.Id == id).SingleOrDefault();

            aspnetUser.Firstname = stagiaire.Prenom;
            aspnetUser.LastName = stagiaire.Nom;

            aspnetUser.DateDebutParticipation = stagiaire.DateDebutParticipation;
            
            if (stagiaire.TypeAffiliationId == 0)
            {
                aspnetUser.TypeAffiliationId = null;
            }
            else
            {
                aspnetUser.TypeAffiliationId = stagiaire.TypeAffiliationId;
            }


            _context.Entry(aspnetUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AspNetUserExists(id))
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


// 1. L'API pour mettre à jour (Sauvegarde dans l'Historique + Sur le Stagiaire)
[HttpPost("{id}/level")]
public async Task<IActionResult> UpdateLevel(string id, [FromBody] LevelUpdateDto dto)
{
    // A. Mise à jour de l'historique (Table StagiaireNiveau)
    var niveau = await _context.StagiaireNiveaux
        .SingleOrDefaultAsync(n => n.StagiaireId == id && n.Annee == dto.Year && n.Trimestre == dto.Quarter);

    if (niveau == null)
    {
        niveau = new StagiaireNiveau { StagiaireId = id, Annee = dto.Year, Trimestre = dto.Quarter };
        _context.StagiaireNiveaux.Add(niveau);
    }

    if (dto.Field == "oralLevel") niveau.OralLevel = dto.Value;
    else if (dto.Field == "writtenLevel") niveau.WrittenLevel = dto.Value;

    // B. Mise à jour "Legacy" sur l'Utilisateur (Pour que les PDF et le reste marchent sans modif)
    // C'est l'astuce qui vous sauve les erreurs de build !
    var user = await _context.Users.FindAsync(id);
    if (user != null)
    {
        if (dto.Field == "oralLevel") user.OralLevel = dto.Value;
        else if (dto.Field == "writtenLevel") user.WrittenLevel = dto.Value;
    }

    await _context.SaveChangesAsync();
    return Ok();
}

// 2. L'API pour lire l'historique (Utilisée par votre nouveau tableau VueJS)
[HttpGet("levels")]
public async Task<ActionResult<IEnumerable<LevelInfoByPeriod>>> GetLevelsByQuarter([FromQuery] int year, [FromQuery] int quarter)
{
    var data = await _context.StagiaireNiveaux
        .AsNoTracking()
        .Where(n => n.Annee == year && n.Trimestre == quarter)
        .Select(n => new LevelInfoByPeriod 
        { 
            StagiaireId = n.StagiaireId, 
            OralLevel = n.OralLevel, 
            WrittenLevel = n.WrittenLevel 
        })
        .ToListAsync();

    return Ok(data);
}



        // DELETE: api/Stagiaires/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApplicationUser>> DeleteStagiaire(string id)
        {
            var user = await _context.Users.Include(a => a.StageStagiaires).Where(a => a.Id == id).SingleOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }
        private bool AspNetUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
