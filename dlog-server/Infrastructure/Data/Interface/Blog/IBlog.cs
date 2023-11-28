using dlog_server.Infrastructure.Models.Blog;
using dlog.server.Infrasructure.Models.Helpers;
using dlog.server.Infrasructure.Models.Returns;

namespace dlog_server.Infrastructure.Data.Interface.Blog
{
    public interface IBlog
    {
        Task<Posts>? Get(int? ID, string? Title);
        Task<PostReturn>? GetView(int? ID, string? Title);
        Task<IEnumerable<PostReturn>>? GetRecentPosts();
        Task<FilteredList<Posts>> FilterPosts(Filter filter);
        Task<Posts>? ManagePost(int UserID, Posts entity);
        Task<bool>? ToggleVisibility(Posts entity);
    }
}
