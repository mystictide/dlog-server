using dlog_server.Infrastructure.Models.Users;

namespace dlog.server.Infrasructure.Models.Returns
{
    public class UserReturn
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public UserSettings? Settings { get; set; }
    }
}
