using Newtonsoft.Json;

namespace dlog_server.Infrastructure.Models.Users
{
    public class UserSettings
    {
        private string? _picture;
        [JsonProperty(PropertyName = "picture")]
        public string? Picture
        {
            get { return _picture; }
            set { _picture = string.IsNullOrEmpty(value) ? null : value; }
        }
        private string? _bio;
        [JsonProperty(PropertyName = "bio")]
        public string? Bio
        {
            get { return _bio; }
            set { _bio = string.IsNullOrEmpty(value) ? null : value; }
        }
        private string _facebook;
        [JsonProperty(PropertyName = "facebook")]
        public string? Facebook
        {
            get { return _facebook; }
            set { _facebook = string.IsNullOrEmpty(value) ? null : value; }
        }
        private string _instagram;
        [JsonProperty(PropertyName = "instagram")]
        public string? Instagram
        {
            get { return _instagram; }
            set { _instagram = string.IsNullOrEmpty(value) ? null : value; }
        }
        private string? _twitter;
        [JsonProperty(PropertyName = "twitter")]
        public string? Twitter
        {
            get { return _twitter; }
            set { _twitter = string.IsNullOrEmpty(value) ? null : value; }
        }
        private string? _linkedin;
        [JsonProperty(PropertyName = "linkedin")]
        public string? Linkedin
        {
            get { return _linkedin; }
            set { _linkedin = string.IsNullOrEmpty(value) ? null : value; }
        }
        private string? _youtube;
        [JsonProperty(PropertyName = "youtube")]
        public string? YouTube
        {
            get { return _youtube; }
            set { _youtube = string.IsNullOrEmpty(value) ? null : value; }
        }
        private string? _personal;
        [JsonProperty(PropertyName = "personal")]
        public string? Personal
        {
            get { return _personal; }
            set { _personal = string.IsNullOrEmpty(value) ? null : value; }
        }
    }
}