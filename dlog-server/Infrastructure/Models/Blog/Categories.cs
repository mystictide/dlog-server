using Dapper.Contrib.Extensions;

namespace dlog_server.Infrastructure.Models.Blog
{
    [Table("categories")]
    public class Categories
    {
        [Key]
        public int? ID { get; set; }
        public string? Name { get; set; }
    }
}
