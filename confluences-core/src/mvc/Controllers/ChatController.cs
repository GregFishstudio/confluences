using Confluences.Infrastructure.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace mvc.Controllers
{
    public class ChatController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IConfiguration configuration, IHubContext<ChatHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        //public async Task<IActionResult> SendMessageClassRoom(string message, string roomName)
        //{
        // await _hubContext.Clients.Group($"{roomName}").SendAsync("ChatMessage", message, $"{roomName}", User.Identity.Name);
        //}

        [HttpPost]
        public async Task<IActionResult> SendMessageToClassRoom(string message, string userName)
        {
            string transformedMessage = $"{userName} : " + message;
            await _hubContext.Clients.All.SendAsync("SendMessage", transformedMessage);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> StudentSendMessageToClassRoom(string message, string userName) {
            string transformedMessage = $"{userName} : " + message;
            await _hubContext.Clients.All.SendAsync("SendStudentMessage", transformedMessage);
            return Ok();
        }
    }
}
