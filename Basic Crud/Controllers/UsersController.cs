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
            (User? user, bool loggedIn, bool userExist) = await service.GetUserDetails();

            return ResponseToClient(user, loggedIn, userExist);
        }

        [HttpPut("update-details")]
        public async Task<ActionResult<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            bool usernameAvailable = await service.IsUsernameAvailable(userUpdateDto.Username);
            if (!usernameAvailable) return BadRequest("Username is not available.");

            (User? user, bool loggedIn, bool userExist) = await service.UpdateUser(userUpdateDto);

            return ResponseToClient(user, loggedIn, userExist);
        }

        [HttpPut("update-password")]
        public async Task<ActionResult<User>> UpdatePassword(UserUpdatePassowrd userUpdate)
        {
            if (userUpdate.Password != userUpdate.ConfirmPassword) return BadRequest("Password and Confirm Password does not match");

            (User? user, bool loggedIn, bool userExist) = await service.UpdatePassword(userUpdate);

            return ResponseToClient(user, loggedIn, userExist);
        }

        private ActionResult<User> ResponseToClient(User? user, bool loggedIn, bool userExist)
        {
            if (!loggedIn) return Unauthorized("Could not verify token. Please log in again to perform this action!");

            if (!userExist) return NotFound("User does not exist!");

            return Ok(user);
        }

        [HttpGet("/items")]
        public async Task<ActionResult<List<Category>>> GetUserItems()
        {
            (List<Item> items, bool loggedIn, bool userExist) = await service.GetUserItems();

            if (!loggedIn) return Unauthorized("Please make sure you are logged in before performing this action!");
            if (!userExist) return NotFound("User not found!");

            return Ok(items);
        }

        [HttpGet("/auctions")]
        public async Task<ActionResult<Auction>> GetUserAuctions()
        {
            return Ok(new List<Auction>());
        }
    }
}
