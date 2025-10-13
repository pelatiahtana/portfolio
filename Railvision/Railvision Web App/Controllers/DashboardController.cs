using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RailVision.App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrainGenie.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController(IConfiguration config) : Controller
    {
        private readonly IConfiguration _config = config;

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("IsAuthenticated") != "true")
                return RedirectToAction("Login", "Account");

            return View();
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class WagonDetachmentLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WagonDetachmentLogController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _context.WagonDetachmentLogs
                .OrderByDescending(l => l.Time)
                .Take(100)
                .ToListAsync();
            return Ok(logs);
        }

        [HttpPost]
        public async Task<IActionResult> AddLog([FromBody] WagonDetachmentLog log)
        {
            log.Time = DateTime.UtcNow;
            log.UserName = _httpContextAccessor.HttpContext.User.Identity.Name ?? "Anonymous";
            _context.WagonDetachmentLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearLogs()
        {
            _context.WagonDetachmentLogs.RemoveRange(_context.WagonDetachmentLogs);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

}