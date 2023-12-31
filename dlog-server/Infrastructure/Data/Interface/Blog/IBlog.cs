using dlog_server.Infrastructure.Models.Blog;
using dlog.server.Infrasructure.Models.Helpers;
using dlog.server.Infrasructure.Models.Returns;
using dlog_server.Infrastructure.Models.Returns;

namespace dlog_server.Infrastructure.Data.Interface.Blog
{
    public interface IBlog
    {
        Task<Posts>? Get(int? ID, string? Title);
        Task<Comments>? GetComment(int ID);
        Task<PostReturn>? GetView(int? ID, string? Title, int? UserID);
        Task<IEnumerable<PostReturn>>? GetRecentPosts(bool isMedia);
        Task<IEnumerable<PostReturn>>? GetRandomPosts();
        Task<FilteredList<CommentReturn>>? FilterComments(Filter filter, int? UserID);
        Task<PostStatistics>? GetPostStatistics(int ID);
        Task<CommentStatistics>? GetCommentStatistics(int ID);
        Task<FilteredList<PostReturn>> FilterPosts(Filter filter);
        Task<bool>? ToggleVisibility(Posts entity);
        Task<Posts>? ManagePost(int UserID, Posts entity);
        Task<Comments>? ManageComment(int UserID, Comments entity);
        Task<bool?> ManagePostVote(int UserID, int PostID, bool? vote);
        Task<bool?> ManageCommentVote(int UserID, int CommentID, bool? vote);
    }
}
