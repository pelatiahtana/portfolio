using Microsoft.AspNetCore.SignalR;
using TrainGenie.Models;

namespace TrainGenie.Hubs
{
    public class IncidentHub : Hub
    {
        public async Task NotifyNewIncident(Incident incident)
        {
            await Clients.Group("Admins").SendAsync("ReceiveNewIncident", incident);
        }

        public async Task NotifyIncidentUpdate(Incident incident)
        {
            await Clients.Group("Admins").SendAsync("ReceiveIncidentUpdate", incident);
        }

        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }
    }
}