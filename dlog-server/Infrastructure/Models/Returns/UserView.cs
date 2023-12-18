using Newtonsoft.Json;
using dlog_server.Infrastructure.Models.Blog;
using dlog_server.Infrastructure.Models.Users;
using dlog.server.Infrasructure.Models.Returns;

namespace dlog_server.Infrastructure.Models.Returns
{
    public class UserView
    {
        [JsonProperty(PropertyName = "UID")]
        public int ID { get; set; }
        public string? Username { get; set; }
        public string? UserImage
        {
            get
            {
                return "https://dapi.herrguller.cc/static/avatars/user/" + ID + "/ua-small.jpg";
            }
        }
        public UserSettings? Socials { get; set; }
        public IEnumerable<Posts>? RecentPosts { get; set; }
        public IEnumerable<Posts>? RecentMedia { get; set; }
        public IEnumerable<UserReturn>? Following { get; set; }
        public IEnumerable<UserReturn>? Followers { get; set; }
        public UserStatistics? UserStatistics { get; set; }
    }
}
