using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Intern_Management.Data;
using Intern_Management.Models;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupervisorController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("GetAssignedCandidates")]
        public IActionResult GetAssignedCandidates()
        {
            // Récupérer l'ID du superviseur actuellement connecté à partir des claims d'identité
            var supervisorIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(supervisorIdString) || !int.TryParse(supervisorIdString, out var supervisorId))
            {
                return BadRequest("Supervisor ID not found or invalid format.");
            }

            // Récupérer le superviseur actuellement connecté avec les candidats assignés à ce superviseur
            var supervisor = _context.Supervisors
                .Include(s => s.AssignedCandidates) // Charger les candidats assignés à ce superviseur
                .FirstOrDefault(s => s.Id == supervisorId);

            if (supervisor == null)
            {
                return NotFound("Supervisor not found.");
            }

            return Ok(supervisor.AssignedCandidates); // Renvoyer les candidats assignés à ce superviseur
        }


    }
}
