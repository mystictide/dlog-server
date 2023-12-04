using Dapper.Contrib.Extensions;

namespace dlog_server.Infrastructure.Models.Blog
{
    [Table("comments")]
    public class Comments
    {
        [Key]
        public int? ID { get; set; }
        public int PostID { get; set; }
        public int UserID { get; set; }
        public string? Body { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
    }
}
