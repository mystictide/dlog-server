using dlog.server.Helpers;
using Microsoft.AspNetCore.Mvc;
using dlog_server.Infrastructure.Models.Blog;
using dlog_server.Infrastructure.Managers.Blog;
using dlog.server.Infrasructure.Models.Helpers;

namespace dlog_server.Controllers
{
    [ApiController]
    [Route("blog")]
    public class BlogController : ControllerBase
    {
        private static int AuthorizedAuthType = 1;

        [HttpPost]
        [Route("toggle/post")]
        public async Task<IActionResult> TogglePost([FromBody] Posts entity)
        {
            try
            {
                var result = await new BlogManager().ToggleVisibility(entity);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpGet]
        [Route("get/post")]
        public async Task<IActionResult> GetPost([FromQuery] int? ID, [FromQuery] string? Title, [FromQuery] bool View)
        {
            try
            {
                if (View)
                {
                    var UserID = AuthHelpers.CurrentUserID(HttpContext);
                    return Ok(await new BlogManager().GetView(ID, Title, UserID));
                }
                var result = await new BlogManager().Get(ID, Title);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpGet]
        [Route("manage/post")]
        public async Task<IActionResult> ManagePost([FromQuery] int? ID, [FromQuery] string? Title)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var result = await new BlogManager().Get(ID, Title);
                    var UserID = AuthHelpers.CurrentUserID(HttpContext);
                    if (UserID == result.UserID)
                    {
                        return Ok(result);
                    }
                    return StatusCode(401, "Access denied");
                }
                return StatusCode(401, "Authorization failed");
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("manage/post")]
        public async Task<IActionResult> ManagePost([FromBody] Posts entity)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var post = await new BlogManager().Get(entity.ID, null);
                    var UserID = AuthHelpers.CurrentUserID(HttpContext);
                    if (entity.ID == null || UserID == post?.UserID)
                    {
                        var result = await new BlogManager().ManagePost(UserID, entity);
                        return Ok(result);
                    }
                    return StatusCode(401, "Access denied");
                }
                return StatusCode(401, "Authorization failed");
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("manage/comment")]
        public async Task<IActionResult> ManageComment([FromBody] Comments entity)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var comment = await new BlogManager().GetComment(entity.ID ?? 0);
                    var UserID = AuthHelpers.CurrentUserID(HttpContext);
                    if (entity.ID == null || UserID == comment?.UserID)
                    {
                        var result = await new BlogManager().ManageComment(UserID, entity);
                        return Ok(result);
                    }
                    return StatusCode(401, "Access denied");
                }
                return StatusCode(401, "Authorization failed");
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }

        [HttpPost]
        [Route("manage/vote/post")]
        public async Task<IActionResult> ManagePostVote([FromQuery] int PostID, [FromQuery] bool? vote)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var voteres = await new BlogManager().ManagePostVote(AuthHelpers.CurrentUserID(HttpContext), PostID, vote);
                    var stats = await new BlogManager().GetPostStatistics(PostID);
                    var result = new { vote = voteres, votecount = stats };
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
        [Route("manage/vote/comment")]
        public async Task<IActionResult> ManageCommentVote([FromQuery] int CommentID, [FromQuery] bool? vote)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var result = await new BlogManager().ManageCommentVote(AuthHelpers.CurrentUserID(HttpContext), CommentID, vote);
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
        [Route("filter/comments")]
        public async Task<IActionResult> FilterComments([FromBody] Filter filter)
        {
            try
            {
                if (AuthHelpers.Authorize(HttpContext, AuthorizedAuthType))
                {
                    var result = await new BlogManager().FilterComments(filter, AuthHelpers.CurrentUserID(HttpContext));
                    return Ok(result);
                }
                else
                {
                    var result = await new BlogManager().FilterComments(filter, 0);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }
    }
}
