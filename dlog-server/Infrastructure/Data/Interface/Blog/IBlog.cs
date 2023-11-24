using dlog_server.Infrastructure.Models.Blog;
using dlog.server.Infrasructure.Models.Helpers;

namespace dlog_server.Infrastructure.Data.Interface.Blog
{
    public interface IBlog
    {
        Task<Posts>? Get(int? ID, string? Title);
        Task<IEnumerable<Posts>>? GetRecentPosts();
        Task<FilteredList<Posts>> FilterPosts(Filter filter);
        Task<Posts>? ManagePost(int UserID, Posts entity);
        Task<bool>? ToggleVisibility(Posts entity);
    }
}
