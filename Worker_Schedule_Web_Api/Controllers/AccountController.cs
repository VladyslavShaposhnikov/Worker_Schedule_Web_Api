using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.Authentication;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AccountController(IAuthService authService) : ControllerBase
    {
        [HttpPost]
        [Route("register-worker")]
        public async Task<IActionResult> RegisterWorker([FromBody] RegisterUserDto registerUserDto)
        {
            return await HandleAuthRequest(async () => await authService.Register(registerUserDto, AppRoles.Worker));
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [HttpPost]
        [Route("register-manager")]
        public async Task<IActionResult> RegisterManager([FromBody] RegisterUserDto registerUserDto)
        {
            return await HandleAuthRequest(async () => await authService.Register(registerUserDto, AppRoles.Manager));
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [HttpPost]
        [Route("register-visual-merchandiser")]
        public async Task<IActionResult> RegisterVisualMerchandiser([FromBody] RegisterUserDto registerUserDto)
        {
            return await HandleAuthRequest(async () => await authService.Register(registerUserDto, AppRoles.VisualMerchandiser));
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            return await HandleAuthRequest(async () => await authService.Login(loginUserDto));
        }

        private async Task<IActionResult> HandleAuthRequest(Func<Task<ResultAuthDto>> func)
        {
            var result = await func();
            return result.Success ? Ok(result.Token) : BadRequest(result.Errors);
        }
    }
}
