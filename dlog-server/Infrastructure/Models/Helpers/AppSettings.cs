using Npgsql;
using System.Data;

namespace dlog.server.Infrastructure.Models.Helpers
{
    public class AppSettings
    {
        public string? Secret { get; set; }

        public static string GetSecret()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: false)
                .Build();
            return configuration.GetSection("AppSettings").GetSection("Secret").Value;
        }

        public static string GetConnectionString()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: false)
                .Build();
            return configuration.GetSection("AppSettings").GetSection("connectionString").Value;
        }

        public static IDbConnection GetConnection
        {
            get
            {
                var conn = new NpgsqlConnection(GetConnectionString());
                return conn;
            }
        }
    }
}
