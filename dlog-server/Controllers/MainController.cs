using dlog.server.Helpers;
using Microsoft.AspNetCore.Mvc;
using dlog_server.Infrastructure.Managers.Blog;
using dlog.server.Infrasructure.Models.Helpers;
using dlog.server.Infrastructure.Managers.Users;

namespace dlog.server.Controllers
{
    [ApiController]
    [Route("main")]
    public class MainController : ControllerBase
    {
        [HttpGet]
        [Route("recent/post")]
        public async Task<IActionResult> GetRecentPosts([FromQuery] bool isMedia)
        {
            try
            {
                var result = await new BlogManager().GetRecentPosts(isMedia);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpGet]
        [Route("random/post")]
        public async Task<IActionResult> GetRandomPosts()
        {
            try
            {
                var result = await new BlogManager().GetRandomPosts();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpGet]
        [Route("random/users")]
        public async Task<IActionResult> GetRandomUsers()
        {
            try
            {
                var result = await new UserManager().GetRandomUsers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("filter/posts")]
        public async Task<IActionResult> FilterPosts([FromBody] Filter filter)
        {
            try
            {
                var result = await new BlogManager().FilterPosts(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpGet]
        [Route("get/user")]
        public async Task<IActionResult> GetUser([FromQuery] string Username)
        {
            try
            {
                int? UserID = AuthHelpers.CurrentUserID(HttpContext);
                var result = await new UserManager().ViewUser(Username, UserID ?? 0);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }
    }
}
