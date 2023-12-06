using dlog_server.Infrastructure.Models.Blog;
using dlog.server.Infrasructure.Models.Helpers;
using dlog.server.Infrasructure.Models.Returns;
using dlog.server.Infrastructure.Models.Helpers;
using dlog_server.Infrastructure.Data.Repo.Blog;
using dlog_server.Infrastructure.Data.Interface.Blog;

namespace dlog_server.Infrastructure.Managers.Blog
{
    public class BlogManager : AppSettings, IBlog
    {
        private readonly IBlog _repo;
        public BlogManager()
        {
            _repo = new BlogRepository();
        }

        public async Task<FilteredList<PostReturn>> FilterPosts(Filter filter)
        {
            return await _repo.FilterPosts(filter);
        }

        public async Task<Posts>? Get(int? ID, string? Title)
        {
            return await _repo.Get(ID, Title);
        }

        public async Task<Comments>? GetComment(int ID)
        {
            return await _repo.GetComment(ID);
        }

        public async Task<PostReturn>? GetView(int? ID, string? Title)
        {
            return await _repo.GetView(ID, Title);
        }

        public async Task<IEnumerable<PostReturn>>? GetRecentPosts()
        {
            return await _repo.GetRecentPosts();
        }

        public async Task<bool>? ToggleVisibility(Posts entity)
        {
            return await _repo.ToggleVisibility(entity);
        }

        public async Task<Posts>? ManagePost(int UserID, Posts entity)
        {
            return await _repo.ManagePost(UserID, entity);
        }

        public async Task<Comments>? ManageComment(int UserID, Comments entity)
        {
            return await _repo.ManageComment(UserID, entity);
        }

        public async Task<bool?> ManagePostVote(int? ID, int UserID, int PostID, bool? vote)
        {
            return await _repo.ManagePostVote(ID, UserID, PostID, vote);
        }

        public async Task<bool?> ManageCommentVote(int? ID, int UserID, int CommentID, bool? vote)
        {
            return await _repo.ManageCommentVote(ID, UserID, CommentID, vote);
        }
    }
}
