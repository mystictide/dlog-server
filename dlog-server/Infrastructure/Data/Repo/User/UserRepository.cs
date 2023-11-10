using Dapper;
using dlog.server.Infrasructure.Models.Users;
using dlog.server.Infrastructure.Models.Helpers;
using dlog.server.Infrastructure.Data.Repo.Helpers;
using dlog.server.Infrastructure.Data.Interface.User;
using dlog.server.Infrasructure.Models.Users.Helpers;

namespace dlog.server.Infrastructure.Data.Repo.User
{
    public class UserRepository : AppSettings, IUsers
    {
        public async Task<Users>? Register(Users entity)
        {
            ProcessResult result = new ProcessResult();
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Email", entity.Email);
                param.Add("@Username", entity.Username);
                param.Add("@Password", entity.Password);

                string query = $@"
                INSERT INTO users (email, username, password, authtype, isactive)
	                VALUES (@Email, @Username, @Password, 1, true)
                RETURNING *;";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<Users>(query, param);
                    return res;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.State = ProcessState.Error;
                return null;
            }
        }

        public async Task<Users>? Login(Users entity)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                string WhereClause = $"WHERE (t.email like '%{entity.Email}%');";

                string query = $@"
                SELECT *
                FROM users t
                {WhereClause};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<Users>(query, param);
                    return res;
                }
            }
            catch (Exception ex)
            {
                LogsRepository.CreateLog(ex);
                return null;
            }
        }

        public async Task<bool> CheckEmail(string Email, int? UserID)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@UserID", UserID);
            param.Add("@Email", Email);

            string Query;
            if (UserID.HasValue)
            {
                Query = @"
                SELECT CASE WHEN COUNT(id) > 0 THEN 1 ELSE 0 END
                FROM users 
                WHERE email = @Email AND NOT (id = @UserID);";
            }
            else
            {
                Query = @"
                SELECT CASE WHEN COUNT(id) > 0 THEN 1 ELSE 0 END
                FROM users 
                WHERE email = @Email;";
            }

            using (var con = GetConnection)
            {
                var res = await con.QueryAsync<bool>(Query, param);
                return res.FirstOrDefault();
            }
        }

        public async Task<bool> CheckUsername(string Username, int? UserID)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@UserID", UserID);
            param.Add("@Username", Username);

            string Query;
            if (UserID.HasValue)
            {
                Query = @"
                SELECT CASE WHEN COUNT(id) > 0 THEN 1 ELSE 0 END
                FROM users 
                WHERE username = @Username AND NOT (id = @UserID);";
            }
            else
            {
                Query = @"
                SELECT CASE WHEN COUNT(id) > 0 THEN 1 ELSE 0 END
                FROM users 
                WHERE username = @Username;";
            }

            using (var con = GetConnection)
            {
                var res = await con.QueryAsync<bool>(Query, param);
                return res.FirstOrDefault();
            }
        }

        public async Task<bool>? DeactivateAccount(int ID)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@ID", ID);

                string query = $@"
                UPDATE users
                SET isactive = 0
                WHERE id = @ID;";

                using (var connection = GetConnection)
                {
                    await connection.QueryAsync<ProcessResult>(query, param);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogsRepository.CreateLog(ex);
                return false;
            }
        }

        public async Task<Users>? Get(int? ID, string? Username)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@ID", ID);

                string WhereClause = $" WHERE t.id = @ID OR (t.username like '%{Username}%')";

                string query = $@"
                SELECT *
                FROM users t
                {WhereClause};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<Users>(query, param);
                    return res;
                }
            }
            catch (Exception ex)
            {
                LogsRepository.CreateLog(ex);
                return null;
            }
        }

        public async Task<string>? UpdateEmail(int ID, string Email)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@ID", ID);
                param.Add("@Email", Email);

                string query = $@"
                UPDATE users
                SET email = @Email
                WHERE id = @ID
                RETURNING email;";

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<string>(query, param);
                    return res;
                }
            }
            catch (Exception ex)
            {
                LogsRepository.CreateLog(ex);
                return null;
            }
        }

        public async Task<bool>? ChangePassword(int UserID, string currentPassword, string newPassword)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@ID", UserID);

                string query = $@"
                UPDATE users
                SET password = '{newPassword}'
                WHERE id = @ID;";

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryAsync(query, param);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogsRepository.CreateLog(ex);
                return false;
            }
        }

        public async Task<bool>? UpdateUsername(int ID, string Username)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@ID", ID);
                param.Add("@Username", Username);

                string query = $@"
                UPDATE users
                SET username = @Username
                WHERE id = @ID;";

                using (var connection = GetConnection)
                {
                    await connection.QueryFirstOrDefaultAsync<ProcessResult>(query, param);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogsRepository.CreateLog(ex);
                return false;
            }
        }
    }
}