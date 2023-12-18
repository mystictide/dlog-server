namespace dlog_server.Infrastructure.Models.Returns
{
    public class UserStatistics
    {
        public int PostsCount { get; set; }
        public int FollowingCount { get; set; }
        public int FollowersCount { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsFollowingYou { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsBlockedYou { get; set; }
    }
}