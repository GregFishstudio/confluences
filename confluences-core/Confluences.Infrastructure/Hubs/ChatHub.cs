using IdentityModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confluences.Infrastructure.Hubs
{
    public class ChatHub : Hub
    {
        public static readonly string Url = "/chathub";
        private const string Unknown = nameof(Unknown);
        private static readonly List<string> ConnectedUsers = new List<string>();

        private string ConnectionId = string.Empty;

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.Identity?.Name?.ToUpper() ?? string.Empty;

            if(!ConnectedUsers.Any(x => x == userName)){
                ConnectedUsers.Add(userName);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, userName);

            ConnectionId = Context.ConnectionId;

            await Clients.All.SendAsync("UserConnected", userName);

            // Send the updated list of connected users to all clients
            await UpdateConnectedUsers();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.Identity?.Name?.ToUpper() ?? string.Empty;
            ConnectedUsers.Remove(userName);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userName);

            await Clients.All.SendAsync("UserDisconnected", userName);

            // Send the updated list of connected users to all clients
            await UpdateConnectedUsers();

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            var userName = Context.User.Claims.Where(x => x.Type == JwtClaimTypes.GivenName).FirstOrDefault()?.Value;
            string transformedMessage = $"{userName}" + message;
            await Clients.All.SendAsync("SendMessage", userName, message);
        }

        public async Task SendStudentMessage(string message)
        {
            var userName = Context.User.Claims.Where(x => x.Type == JwtClaimTypes.GivenName).FirstOrDefault()?.Value;
            string transformedMessage = $"{userName}" + message;
            await Clients.All.SendAsync("SendStudentMessage", userName, message);
        }

        public async Task MessageChat(string message, string userName)
        {
            await Clients.All.SendAsync("ClassRoomMessage", message, userName);
        }

        public string getConnexionId()
        {
            return ConnectionId;
        }

        // Send the updated list of connected users to all clients
        private async Task UpdateConnectedUsers()
        {
            await Clients.All.SendAsync("ReceiveConnectedUsers", ConnectedUsers);
        }
    }
}
