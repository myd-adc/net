using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ProposalsController : ControllerBase
{
    private readonly ApplicationDbContext _context; 

    public ProposalsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DataPage<ProposalDto>>> GetAllProposals(
        [FromQuery(Name = "$top")] int? top, 
        [FromQuery(Name = "$skip")] int? skip, 
        [FromQuery(Name = "$filter")] string? filter, 
        [FromQuery(Name = "$orderby")] string? orderby)
    {
        // Базовий запит до таблиці Proposals
        IQueryable<Proposal> query = _context.Proposals;

        // Фільтрування (якщо є фільтр)
        if (!string.IsNullOrEmpty(filter))
        {
            try
            {
                query = query.Where(filter); 
            }
            catch (ParseException ex)
            {
                return BadRequest($"Invalid filter syntax: {ex.Message}");
            }
        }

       
        var totalRecords = await query.CountAsync();


        if (!string.IsNullOrEmpty(orderby))
        {
            try
            {
                query = query.OrderBy(orderby); 
            }
            catch (ParseException ex)
            {
                return BadRequest($"Invalid orderby syntax: {ex.Message}");
            }
        }


        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (top.HasValue)
        {
            query = query.Take(top.Value);
        }

  
        var proposals = await query.Select(p => new ProposalDto 
        { 
            Id = p.Id, 
            Title = p.Title, 
            Description = p.Description 
           
        }).ToListAsync();

      
        return Ok(new DataPage<ProposalDto>
        {
            Data = proposals,
            TotalRecords = totalRecords
        });
    }
}
