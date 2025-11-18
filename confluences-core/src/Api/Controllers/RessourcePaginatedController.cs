using Confluences.Domain.Entities.Views;
using Confluences.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Api.Utility.Pagination.DynamicPagination;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RessourcePaginatedController : ControllerBase
    {
        private readonly ConfluencesDbContext _context;

        public RessourcePaginatedController(ConfluencesDbContext context)
        {
            _context = context;
        }
        // POST: api/RessourcePaginated
        [HttpPost]
        public async Task<object> GetHomeworkV2s([FromForm] DataTableAjaxPostModel model)
       {
            // action inside a standard controller
            int filteredResultsCount = 0;
            int totalResultsCount = await _context.Ressources.CountAsync();

            //var res = DSearchFunc(_context, typeof(Ressource), model, out filteredResultsCount, out totalResultsCount);
            var query = _context.Ressources.AsQueryable();
            if (!string.IsNullOrEmpty(model.search.value))
            {
                var text = model.search.value.ToLower();
                var likePattern = model.search.value.ToLower().Replace(" ", "%");
                likePattern = likePattern + "%";
                query = query.Where(x =>
                    x.SessionNumber.ToLower().Contains(text) ||
                    x.HomeworkTypeName.ToLower().Contains(text) ||
                    EF.Functions.Like(x.Name.ToLower(), likePattern) ||
                    x.Teacher.ToLower().Contains(text) ||
                    x.Date == null ? true : x.Date.Value.ToString().Contains(text));
            }
            var res = await query.OrderByDescending(x => x.Date).Skip(model.start).Take(model.length).ToListAsync();

            filteredResultsCount = await query.CountAsync();

            return new
            {
                // this is what datatables wants sending back
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = res
            };
        }
    }
}
