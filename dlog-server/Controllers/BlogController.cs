﻿using dlog.server.Helpers;
using Microsoft.AspNetCore.Mvc;
using dlog_server.Infrastructure.Models.Blog;
using dlog.server.Infrasructure.Models.Helpers;
using dlog_server.Infrastructure.Managers.Blog;

namespace dlog_server.Controllers
{
    [ApiController]
    [Route("blog")]
    public class BlogController : ControllerBase
    {
        private static int AuthorizedAuthType = 1;

        [HttpGet]
        [Route("get/post")]
        public async Task<IActionResult> GetPost([FromQuery] int? ID, [FromQuery] string? Title)
        {
            try
            {
                var result = await new BlogManager().Get(ID, Title);
                return Ok(result);
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
                    var UserID = AuthHelpers.CurrentUserID(HttpContext);
                    if (UserID == entity.UserID)
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
