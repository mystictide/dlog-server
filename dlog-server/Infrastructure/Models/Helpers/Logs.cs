using Dapper.Contrib.Extensions;

namespace dlog.server.Infrasructure.Models.Helpers
{
    [Table("logs")]
    public class Logs
    {
        [Key]
        public int ID { get; set; }
        public int? UserID { get; set; }
        public string? Message { get; set; }
        public string? Source { get; set; }
        public int Line { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
