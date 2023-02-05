using Basic_Crud.Models;
using Basic_Crud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Basic_Crud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService auth;

        public AuthController(AuthService auth)
        {
            this.auth = auth;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto userReq)
        {
            if (userReq.Password != userReq.ConfirmPassword)
                return BadRequest("Password and Confirm Password do not match.");

            var user = await auth.RegisterUser(userReq);

            if (user == null)
                return Conflict("Username is already taken");

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLogin userLogin)
        {
            var res = await auth.LoginUser(userLogin, Response);
            
            if (res == null)
                return BadRequest("Wrong username or password");

            return Ok(res);
        }

        [HttpGet("refresh-token")]
        [Authorize]
        public async Task<ActionResult<string>> RefreshToken()
        {
            (User? user, bool loggedIn, bool userExists, bool validToken, bool tokenNotExpired, string token) = await auth.RefreshToken(Request, Response);
            if (!loggedIn) return Unauthorized("Please log in again to perform this action");
            if (!userExists) return NotFound("User does not exists!");
            if (!validToken) return Unauthorized("The refresh token is not valid, please log in again");
            if (!tokenNotExpired) return Unauthorized("Your token is expired!");
            return Ok(token);
        }
    }
}
