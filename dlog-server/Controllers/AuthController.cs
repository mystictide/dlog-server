using dlog.server.Helpers;
using Microsoft.AspNetCore.Mvc;
using dlog.server.Infrasructure.Models.Users;
using dlog.server.Infrasructure.Models.Returns;
using dlog.server.Infrastructure.Managers.Users;

namespace dlog.server.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] Users user)
        {
            try
            {
                var data = await new UserManager().Register(user);
                var userData = new UserReturn();
                userData.Username = data.Username;
                userData.Email = data.Email;
                userData.Token = data.Token;
                return Ok(userData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] Users user)
        {
            try
            {
                var data = await new UserManager().Login(user);
                var userData = new UserReturn();
                userData.Username = data.Username;
                userData.Email = data.Email;
                userData.Token = data.Token;
                return Ok(userData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("cmail")]
        public async Task<IActionResult> CheckExistingEmail([FromBody] string email)
        {
            try
            {
                bool exists;
                var userID = AuthHelpers.CurrentUserID(HttpContext);
                if (userID < 1)
                {
                    exists = await new UserManager().CheckEmail(email, null);
                }
                else
                {
                    exists = await new UserManager().CheckEmail(email, userID);
                }
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("cusername")]
        public async Task<IActionResult> checkExistingUsername([FromBody] string username)
        {
            try
            {
                bool exists;
                var userID = AuthHelpers.CurrentUserID(HttpContext);
                if (userID < 1)
                {
                    exists = await new UserManager().CheckUsername(username, null);
                }
                else
                {
                    exists = await new UserManager().CheckUsername(username, userID);
                }

                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
