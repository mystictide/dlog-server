using Newtonsoft.Json;
using Dapper.Contrib.Extensions;
using dlog_server.Infrastructure.Models.Blog;
using dlog_server.Infrastructure.Models.Users;

namespace dlog_server.Infrastructure.Models.Returns
{
    public class CommentReturn
    {
        [Key]
        public int? ID { get; set; }
        public int PostID { get; set; }
        [JsonProperty(PropertyName = "UID")]
        public int UserID { get; set; }
        public string? Body { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? UpdateDate { get; set; }
        public UserSettings? AuthorSocials { get; set; }
        public string? Author { get; set; }
        public string? AuthorImage
        {
            get
            {
                return AuthorSocials?.Picture?.Length > 0 ? "https://dapi.herrguller.cc/static/avatars/user/" + UserID + "/ua-small.jpg" : null;
            }
        }
        public bool? UserVote { get; set; }
        public CommentStatistics? Votes { get; set; }
    }
}
