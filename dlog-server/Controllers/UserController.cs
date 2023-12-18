using dlog.server.Helpers;
using Microsoft.AspNetCore.Mvc;
using dlog_server.Infrastructure.Models.Users;
using dlog.server.Infrastructure.Managers.Users;
using dlog_server.Infrastructure.Models.Helpers;

namespace dlog.server.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private IWebHostEnvironment _env;
        private static int AuthorizedAuthType = 1;

        public UserController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        [Route("manageAvatar")]
        public async Task<IActionResult> ManageAvatar([FromForm] IFormFile file)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    string result = "";
                    if (file.Length > 0)
                    {
                        var path = await CustomHelpers.SaveUserAvatar(AuthHelpers.CurrentUserID(HttpContext), _env.ContentRootPath, file);
                        if (path != null)
                        {
                            result = await new UserManager().ManageAvatar(path, AuthHelpers.CurrentUserID(HttpContext));
                        }
                        else
                        {
                            return StatusCode(401, "Failed to save image");
                        }
                    }
                    return Ok(result);
                }
                return StatusCode(401, "Authorization failed");
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("update/password")]
        public async Task<IActionResult> ChangePassword([FromBody] Dictionary<string, string> data)
        {
            try
            {
                string currentPassword = data["currentPassword"];
                string newPassword = data["newPassword"];
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var result = await new UserManager().ChangePassword(AuthHelpers.CurrentUserID(HttpContext), currentPassword, newPassword);
                    return Ok(result);
                }
                else
                {
                    return StatusCode(401, "Authorization failed");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("update/email")]
        public async Task<IActionResult> UpdateEmail([FromBody] string email)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var result = await new UserManager().UpdateEmail(AuthHelpers.CurrentUserID(HttpContext), email);
                    if (result == null)
                    {
                        return StatusCode(401, "Email already in use");
                    }
                    return Ok(result);
                }
                else
                {
                    return StatusCode(401, "Authorization failed");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("update/bio")]
        public async Task<IActionResult> UpdateBio([FromBody] string bio)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var result = await new UserManager().UpdateBio(AuthHelpers.CurrentUserID(HttpContext), bio);
                    return Ok(result);
                }
                else
                {
                    return StatusCode(401, "Authorization failed");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("update/socials")]
        public async Task<IActionResult> UpdateSocials([FromBody] UserSettings entity)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var result = await new UserManager().UpdateSocials(AuthHelpers.CurrentUserID(HttpContext), entity);
                    return Ok(result);
                }
                else
                {
                    return StatusCode(401, "Authorization failed");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("follow")]
        public async Task<IActionResult> ManageFollow([FromBody] UserFunctions entity)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var res = await new UserManager().ManageFollow(entity, AuthHelpers.CurrentUserID(HttpContext));
                    return Ok(res);
                }
                return StatusCode(401, "Authorization failed");
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("block")]
        public async Task<IActionResult> ManageBlock([FromBody] UserFunctions entity)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var res = await new UserManager().ManageBlock(entity, AuthHelpers.CurrentUserID(HttpContext));
                    return Ok(res);
                }
                return StatusCode(401, "Authorization failed");
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }
    }
}
