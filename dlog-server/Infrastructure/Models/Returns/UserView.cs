using Newtonsoft.Json;
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
                return Socials?.Picture?.Length > 0 ? "https://dapi.herrguller.cc/static/avatars/user/" + ID + "/ua-small.jpg" : null;
            }
        }
        public UserSettings? Socials { get; set; }
        public IEnumerable<PostReturn>? RecentPosts { get; set; }
        public IEnumerable<PostReturn>? RecentMedia { get; set; }
        public IEnumerable<UserReturn>? Following { get; set; }
        public IEnumerable<UserReturn>? Followers { get; set; }
        public UserStatistics? UserStatistics { get; set; }
    }
}
