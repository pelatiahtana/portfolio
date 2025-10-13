using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TrainGenie.Hubs;
using TrainGenie.Models;
using TrainGenie.Services;

namespace TrainGenie.Controllers
{
    [Authorize]
    public class IncidentController : Controller
    {
        private readonly IncidentService _incidentService;
        private readonly IHubContext<IncidentHub> _hubContext;

        public IncidentController(IncidentService incidentService, IHubContext<IncidentHub> hubContext)
        {
            _incidentService = incidentService;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Report([FromBody] Incident incident)
        {
            incident.ReportedBy = HttpContext.Session.GetString("Username");
            await _incidentService.CreateIncident(incident);
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveNewIncident", incident);
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Incident incident)
        {
            await _incidentService.UpdateIncident(incident);
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveIncidentUpdate", incident);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Centre()
        {
            var incidents = await _incidentService.GetAllIncidents();
            return View(incidents);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var incidents = await _incidentService.GetAllIncidents();
            var incident = incidents.FirstOrDefault(i => i.Id == id);
            return Json(incident);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ClearAll()
        {
            await _incidentService.ClearAllIncidents();
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveClearAllIncidents");
            return Ok();
        }
    }
}