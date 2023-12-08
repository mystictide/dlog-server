using Newtonsoft.Json;
using dlog_server.Infrastructure.Models.Blog;
using dlog_server.Infrastructure.Models.Users;

namespace dlog.server.Infrasructure.Models.Returns
{
    public class PostReturn
    {
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "UID")]
        public int? UserID { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Category { get; set; }
        public string? Author { get; set; }
        public UserSettings? AuthorSocials { get; set; }
        public PostStatistics? Votes { get; set; }
        public bool? UserVote { get; set; }
        public string? AuthorImage
        {
            get
            {
                if (UserID.HasValue)
                {
                    return "https://dapi.herrguller.cc/static/avatars/user/" + UserID + "/ua-small.jpg";
                }
                else
                {
                    return null;
                }
            }
        }
        public DateTime Date { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsMedia { get; set; }
    }
}
