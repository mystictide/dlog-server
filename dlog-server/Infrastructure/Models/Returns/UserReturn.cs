using dlog_server.Infrastructure.Models.Users;

namespace dlog.server.Infrasructure.Models.Returns
{
    public class UserReturn
    {
        public int? UID { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public UserSettings? Settings { get; set; }
        public string? UserImage
        {
            get
            {
                return Settings?.Picture?.Length > 0 ? "https://dapi.herrguller.cc/static/avatars/user/" + UID + "/ua-small.jpg" : null;
            }
        }
    }
}
