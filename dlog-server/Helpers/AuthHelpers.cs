using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using dlog.server.Infrastructure.Models.Helpers;

namespace dlog.server.Helpers
{
    public class AuthHelpers : AppSettings
    {
        public static bool Authorize(HttpContext context, int AuthorizedAuthType)
        {
            return ValidateToken(ReadBearerToken(context), AuthorizedAuthType);
        }
        public static int CurrentUserID(HttpContext context)
        {
            return ValidateUser(ReadBearerToken(context));
        }
        public static string? ReadBearerToken(HttpContext context)
        {
            try
            {
                string header = (string)context.Request.Headers["Authorization"];
                if (header != null)
                {
                    return header.Substring(7);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static bool ValidateToken(string? encodedToken, int AuthorizedAuthType)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(GetSecret());
                tokenHandler.ValidateToken(encodedToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var authType = int.Parse(jwtToken.Claims.First(x => x.Type == "authType").Value);
                if (authType >= AuthorizedAuthType)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int ValidateUser(string? encodedToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(GetSecret());
                tokenHandler.ValidateToken(encodedToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}