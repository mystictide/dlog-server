using dlog.server.Infrasructure.Models.Users;
using dlog_server.Infrastructure.Models.Users;

namespace dlog.server.Infrastructure.Data.Interface.User
{
    public interface IUsers
    {
        Task<bool> CheckEmail(string Email, int? UserID);
        Task<bool> CheckUsername(string Username, int? UserID);
        Task<Users>? Login(Users entity);
        Task<Users>? Register(Users entity);
        Task<Users>? Get(int? ID, string? Username);
        Task<UserSettings>? GetUserSettings(int? ID, string? Username);
        Task<bool>? ChangePassword(int UserID, string currentPassword, string newPassword);
        Task<string>? UpdateEmail(int ID, string Email);
        Task<string>? UpdateBio(int ID, string Bio);
        Task<UserSettings>? UpdateSocials(int ID, UserSettings entity);
        Task<string>? ManageAvatar(string path, int userID);
        Task<bool>? DeactivateAccount(int ID);
    }
}
