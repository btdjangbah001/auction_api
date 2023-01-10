using Basic_Crud.Models;
using Basic_Crud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var res = await service.GetUserDetails();

            return Response(res);
        }

        [HttpPut("update-details")]
        public async Task<ActionResult<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            bool usernameAvailable = await service.IsUsernameAvailable(userUpdateDto.Username);
            if (!usernameAvailable) return BadRequest("Username is not available.");

            var res = await service.UpdateUser(userUpdateDto);

            return Response(res);
        }

        [HttpPut("update-password")]
        public async Task<ActionResult<User>> UpdatePassword(UserUpdatePassowrd userUpdate)
        {
            var res = await service.UpdatePassword(userUpdate);

            return Response(res);
        }

        private ActionResult<User> Response(Tuple<User?, bool> user)
        {
            if (user.Item2 == false) return Unauthorized("Could not verify token. Please log in again to perform this action!");

            if (user.Item1 == null) return NotFound("User does not exist!");

            return Ok(user.Item1);
        }
    }
}
