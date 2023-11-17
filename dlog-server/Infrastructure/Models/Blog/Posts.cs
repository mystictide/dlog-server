using Dapper.Contrib.Extensions;

namespace dlog_server.Infrastructure.Models.Blog
{
    [Table("posts")]
    public class Posts
    {
        [Key]
        public int? ID { get; set; }
        public int? UserID { get; set; }
        public int? CategoryID { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public DateTime? Date { get; set; }
        public bool? IsActive { get; set; }
    }
}
