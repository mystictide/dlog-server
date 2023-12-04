using Newtonsoft.Json;
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
        public string? AuthorImage
        {
            get
            {
                return "https://dapi.herrguller.cc/static/avatars/user/" + UserID + "/ua-small.jpg";
            }
        }
        public DateTime Date { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsMedia { get; set; }
    }
}
