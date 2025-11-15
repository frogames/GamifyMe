using GamifyMe.Api.Constants;
using GamifyMe.Api.Data;
using GamifyMe.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamifyMe.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Editeur},{Roles.Gestionnaire}")]
    public class DashboardController : ControllerBase
    {
        private readonly DataContext _context;

        public DashboardController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("activity-logs")]
        public async Task<ActionResult<List<DashboardLogDto>>> GetDashboardLogs()
        {
            var establishmentId = Guid.Parse(User.FindFirstValue("EstablishmentId")!);
            var logs = new List<DashboardLogDto>();

            // 1. Récupérer les derniers SCANS
            var recentScans = await _context.Validations
                .Include(v => v.Objective)
                .Where(v => v.EstablishmentId == establishmentId)
                .OrderByDescending(v => v.Date)
                .Take(10)
                .Select(v => new DashboardLogDto
                {
                    Date = v.Date,
                    ActorName = "Gestionnaire",
                    ActionType = "Scan",
                    Details = $"Validation : {v.Objective.Title}",
                    Icon = "QrCode",
                    Color = "Success"
                })
                .ToListAsync();
            logs.AddRange(recentScans);

            // 2. Récupérer les derniers OBJECTIFS créés
            var recentObjectives = await _context.Objectives
                // CORRECTION : On ne peut pas inclure 'CreatedBy' car il n'existe pas sur le modèle
                // .Include(o => o.CreatedBy) 
                .Where(o => o.EstablishmentId == establishmentId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new DashboardLogDto
                {
                    Date = o.CreatedAt,
                    ActorName = "Admin", // On met "Admin" en dur car on n'a pas la jointure
                    ActionType = "Création Objectif",
                    Details = o.Title,
                    Icon = "Trophy",
                    Color = "Warning"
                })
                .ToListAsync();
            logs.AddRange(recentObjectives);

            // 3. Fusionner, Trier et Renvoyer
            var finalLogs = logs
                .OrderByDescending(l => l.Date)
                .Take(20)
                .ToList();

            return Ok(finalLogs);
        }
    }
}