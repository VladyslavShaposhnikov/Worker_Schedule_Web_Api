using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.Authentication;
using Worker_Schedule_Web_Api.Models.Identity;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResultAuthDto> Register(RegisterUserDto registerUserDto, string role);
        Task<ResultAuthDto> Login(LoginUserDto loginUserDto);
    }
}
