using Dapper.Contrib.Extensions;

namespace dlog_server.Infrastructure.Models.Blog
{
    [Table("poststatistics")]
    public class PostStatistics
    {
        [Key]
        public int ID { get; set; }
        public int PostID { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
    }
}
