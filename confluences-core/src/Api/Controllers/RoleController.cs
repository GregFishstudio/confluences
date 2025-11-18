using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public RoleController(ConfluencesDbContext context)
        {
            _context = context;
        }

        // GET: api/Role
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<string>>> GetRole(string userId)
        {
            return await _context.UserRoles
                            .AsNoTracking()
                            .Where(a => a.UserId == userId)
                            .Select(a => a.RoleId)
                            .ToListAsync();
        }

        // GET: api/Role/TeachersHome
        [Route("TeachersHome")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetTeachers()
        {
            List<ApplicationUser> teachers =  await _context.UserRoles
                                                .Include(a => a.User)
                                                .Include(a => a.Role)
                                                .Where(a => a.Role.Name == "Teacher")
                                                .Select(a => a.User)
                                                .ToListAsync();
            teachers = teachers.Where(u => u.UserName != "TA").ToList();

            List<ApplicationUser> teachersCleaned = new List<ApplicationUser>();
            foreach (ApplicationUser teacher in teachers)
            {
                teachersCleaned.Add(new ApplicationUser
                { 
                    Firstname = teacher.Firstname,
                    LastName = teacher.LastName,
                    PathImage = teacher.PathImage,
                    PhoneNumber = teacher.PhoneNumber
                });
            }

            return teachersCleaned;
        }

        [Authorize(Policy = "Teacher")]
        // GET: api/Role/Teachers
        [Route("Teachers")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetTeachersHome()
        {
            return await _context.UserRoles
                            .Include(a => a.User)
                            .Include(a => a.Role)
                            .Where(a => a.Role.Name == "Teacher")
                            .Select(a => a.User)
                            .OrderBy(a => a.Firstname)
                            .ToListAsync();
        }

        [Authorize(Policy = "Teacher")]
        // GET: api/Role/Students
        [Route("Students")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetStudents()
        {
            List<string> usersId = await _context.UserRoles
                            .Include(a => a.User)
                            .Include(a => a.Role)
                            .Where(a => a.Role.Name == "Teacher")
                            .Select(a => a.User.Id)
                            .ToListAsync();

            return await _context.Users
                            .Where(a => !usersId.Contains(a.Id))
                            .OrderBy(a => a.Firstname)
                            .ToListAsync();
        }
    }
}