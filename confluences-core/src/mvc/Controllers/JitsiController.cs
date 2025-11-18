using Confluences.Infrastructure.Hubs;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using mvc.Models;
using Syncfusion.EJ2.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mvc.Controllers
{
    public class JitsiController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<JitsiHub> _hubContext;
        public JitsiController(IConfiguration configuration, IHubContext<JitsiHub> Jitsicontext)
        {
            _configuration = configuration;
            _hubContext = Jitsicontext;
        }

        public async Task<IActionResult> Index(string roomName, bool isPrivateRoom = false, string language = "fr")
        {
            JitsiConfiguration config = new JitsiConfiguration()
            {
                Url = _configuration["Jitsi:Url"],
                Language = language,
                UserFriendlyName = User.Claims.Where(x => x.Type == JwtClaimTypes.GivenName).FirstOrDefault().Value,
            };

            //if (isPrivateRoom)
            //{
            //    config.isPrivateRoom = true;
            //}
            //else
            //{ 
            //    config.isPrivateRoom = false;
            //}

            config.RoomName = $"SchoolClassRoom_{roomName}";

            await _hubContext.Clients.Group($"SchoolClassRoom_{roomName}").SendAsync("JitsiNotification", "Test", $"SchoolClassRoom_{roomName}", User.Identity.Name);


            
            ViewData["URLMVC"] = _configuration["URLClientMVC"];
            return View(config);
        }

        public async Task<IActionResult> Private(string roomName, bool isPrivateRoom = false, string language = "fr")
        {
            JitsiConfiguration config = new JitsiConfiguration()
            {
                Url = _configuration["Jitsi:Url"],
                Language = language,
                UserFriendlyName = User.Claims.Where(x => x.Type == JwtClaimTypes.GivenName).FirstOrDefault().Value,
            };

            //if (isPrivateRoom)
            //{
            //    config.isPrivateRoom = true;
            //}
            //else
            //{ 
            //    config.isPrivateRoom = false;
            //}

            config.RoomName = $"SchoolClassRoom_{roomName}";

            await _hubContext.Clients.Group($"SchoolClassRoom_{roomName}").SendAsync("JitsiPrivateNotification", "Test", $"SchoolClassRoom_{roomName}", User.Identity.Name);



            ViewData["URLMVC"] = _configuration["URLClientMVC"];
            return View(config);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessageClassRoom(string roomName)
        {
            await _hubContext.Clients.Group($"{roomName}").SendAsync("JitsiNotification", "Test", $"{roomName}", User.Identity.Name);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendPrivateMessageClassRoom(string roomName)
        {
            await _hubContext.Clients.Group($"{roomName}").SendAsync("JitsiPrivateNotification", "Test", $"{roomName}", User.Identity.Name);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> StopPrivateClassRoom(string roomName)
        {
            await _hubContext.Clients.Group($"{roomName}").SendAsync("StopPrivateClassRoom", "Test", $"{roomName}", User.Identity.Name);

            return Ok();
        }
    }
}
