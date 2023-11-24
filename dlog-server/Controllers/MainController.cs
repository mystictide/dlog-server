using Microsoft.AspNetCore.Mvc;
using dlog_server.Infrastructure.Managers.Blog;

namespace dlog.server.Controllers
{
    [ApiController]
    [Route("main")]
    public class MainController : ControllerBase
    {
        [HttpGet]
        [Route("recent/post")]
        public async Task<IActionResult> GetRecentPosts()
        {
            try
            {
                var result = await new BlogManager().GetRecentPosts();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }
    }
}
