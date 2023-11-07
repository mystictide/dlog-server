using dlog.server.Helpers;
using Microsoft.AspNetCore.Mvc;
using dlog.server.Infrastructure.Managers.Users;

namespace dlog.server.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {
        private static int AuthorizedAuthType = 1;

        [HttpGet]
        [Route("change/password")]
        public async Task<IActionResult> ChangePassword([FromQuery] string currentPassword, [FromQuery] string newPassword)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var result = await new UserManager().ChangePassword(AuthHelpers.CurrentUserID(HttpContext), currentPassword, newPassword); return Ok(result);
                }
                else
                {
                    return StatusCode(500, "Authorization failed");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("update/email")]
        public async Task<IActionResult> UpdateEmail([FromQuery] string email)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var result = await new UserManager().UpdateEmail(AuthHelpers.CurrentUserID(HttpContext), email);
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, "Authorization failed");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
