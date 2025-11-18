using IdentityModel;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confluences.Infrastructure.Hubs
{
    public class JitsiHub : Hub
    {
        public static readonly string Url = "/jitsihub";

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.Identity?.Name?.ToUpper() ?? string.Empty;

            await Groups.AddToGroupAsync(Context.ConnectionId, userName);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.Identity?.Name?.ToUpper() ?? string.Empty;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userName);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message, string userName)
        {
            await Clients.Group(userName).SendAsync("ReceiveMessage", message);
        }

        public async Task SendNotificationOpenClassRoom(string message, string groupName, string userName)
        {
            await Clients.Group(groupName).SendAsync("JitsiNotification", message, userName);
        }

        public async Task StopClassRoom(string message, string groupName, string userName)
        {
            await Clients.Group(groupName).SendAsync("StopClassRoom", message, userName);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
