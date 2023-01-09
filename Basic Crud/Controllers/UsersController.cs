using Basic_Crud.Models;
using Basic_Crud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Basic_Crud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UsersService service;

        public UsersController(UsersService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<ActionResult<User>> GetUserDetails()
        {
            var username = HttpContext.User.FindFirst(ClaimTypes.Name).Value;

            if (username == null) return Unauthorized("Could not verify token. Please log in again to perform this action!");

            var user = await service.GetUserDetails(username);

            if (user == null) return NotFound("Could not find this user, please log out and log in again");

            return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            var username = HttpContext.User.FindFirst(ClaimTypes.Name).Value;

            if (username == null) return Unauthorized("Could not verify token. Please log in again to perform this action!");

            var user = await service.UpdateUser(userUpdateDto, username);
            return Ok(user);
        }
    }
}
