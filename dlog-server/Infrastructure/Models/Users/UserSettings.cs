using Newtonsoft.Json;

namespace dlog_server.Infrastructure.Models.Users
{
    public class UserSettings
    {
        [JsonProperty(PropertyName = "picture")]
        public string? Picture { get; set; }
        [JsonProperty(PropertyName = "bio")]
        public string? Bio { get; set; }
        [JsonProperty(PropertyName = "facebook")]
        public string? Facebook { get; set; }
        [JsonProperty(PropertyName = "instagram")]
        public string? Instagram { get; set; }
        [JsonProperty(PropertyName = "twitter")]
        public string? Twitter { get; set; }
        [JsonProperty(PropertyName = "linkedin")]
        public string? Linkedin { get; set; }
        [JsonProperty(PropertyName = "personal")]
        public string? Personal { get; set; }
    }
}
