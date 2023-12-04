using Microsoft.AspNetCore.Mvc;
using dlog_server.Infrastructure.Managers.Blog;
using dlog.server.Infrasructure.Models.Helpers;

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
    }
}
