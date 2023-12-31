using Dapper;
using dlog.server.Infrasructure.Models.Users;
using dlog_server.Infrastructure.Models.Blog;
using dlog_server.Infrastructure.Models.Users;
using dlog.server.Infrasructure.Models.Returns;
using dlog.server.Infrastructure.Models.Helpers;
using dlog_server.Infrastructure.Models.Returns;
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
                string usQuery = $@"
                INSERT INTO usersettings (userid, picture, bio, facebook, instagram, linkedin, twitter, youtube, personal)
	                VALUES (@UserID, null, null, null, null, null, null, null, null)";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<Users>(query, param);
                    param.Add("@UserID", res?.ID);
                    await con.QueryFirstOrDefaultAsync<Users>(usQuery, param);
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
                string WhereClause = $"WHERE (t.email like '%{entity.Email}%');";

                string query = $@"
                SELECT *
                FROM users t
                {WhereClause};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<Users>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
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
                await new LogsRepository().CreateLog(ex);
                return false;
            }
        }

        public async Task<Users>? Get(int? ID, string? Username)
        {
            try
            {
                string WhereClause = $" WHERE t.id = {ID ?? 0}  OR (t.username like '%{Username}%')";

                string query = $@"
                SELECT *
                FROM users t
                {WhereClause};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<Users>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<UserView>? ViewUser(string? Username, int? UserID)
        {
            try
            {
                var param = new DynamicParameters();
                string WhereClause = $" WHERE t.username ilike '{Username}'";
                string query = $@"
                SELECT t.*, us.* 
                FROM users t 
                left join usersettings us on us.userid = t.id 
                {WhereClause};";

                string postsQuery = $@"
                SELECT t.*, u.id, u.username, c.*, u2.id, u2.picture
                FROM posts t
                left join users u on u.id = t.userid
                left join categories c on c.id = t.categoryid
                left join usersettings u2 on u2.userid = t.userid
                WHERE t.userid = @UserID and t.ismedia = false and t.isactive = true
                order by COALESCE(t.updatedate, t.date) desc limit 6;";

                string mediaQuery = $@"
                SELECT t.*, u.id, u.username, c.*, u2.id, u2.picture
                FROM posts t
                left join users u on u.id = t.userid
                left join categories c on c.id = t.categoryid
                left join usersettings u2 on u2.userid = t.userid
                WHERE t.userid = @UserID and t.ismedia = true and t.isactive = true
                order by COALESCE(t.updatedate, t.date) desc limit 6;";

                string followingQuery = $@"
                SELECT t.id as uid, t.username, us.* FROM users t 
                left join usersettings us on us.userid = t.id 
                WHERE t.id in (select followedid from userfollowjunction ufj where ufj.followerid = @UserID) limit 15;";

                string followersQuery = $@"
                SELECT t.id as uid, t.username, us.*  FROM users t 
                left join usersettings us on us.userid = t.id 
                WHERE t.id in (select followerid from userfollowjunction ufj where ufj.followedid = @UserID) limit 15;";

                string statsQuery = $@"
                select
                (SELECT count(id) FROM posts p where p.userid = t.id and p.isactive = true) as PostsCount,
                (SELECT count(id) FROM userfollowjunction fn where fn.followerid = t.id) as FollowingCount,
                (SELECT count(id) FROM userfollowjunction fw where fw.followedid = t.id) as FollowersCount,
                (SELECT (case when exists (SELECT id FROM userfollowjunction isf where isf.followerid = {UserID ?? 0} and isf.followedid = t.id)
                    then 1 
                    else 0 
                  end)) as IsFollowing,
                (SELECT (case when exists (SELECT id FROM userfollowjunction isfy where isfy.followerid = t.id and isfy.followedid = {UserID ?? 0})
                    then 1 
                    else 0 
                  end)) as IsFollowingYou,
                (SELECT (case when exists (SELECT id FROM userblockjunction isb where isb.blockerid = {UserID ?? 0} and isb.blockedid = t.id)
                    then 1 
                    else 0 
                  end)) as IsBlocked,
                (SELECT (case when exists (SELECT id FROM userblockjunction isby where isby.blockerid = t.id and isby.blockedid = {UserID ?? 0})
                    then 1 
                    else 0 
                  end)) as IsBlockedYou
                FROM users t WHERE t.id = @UserID;";

                using (var con = GetConnection)
                {
                    var res = await con.QueryAsync<UserView, UserSettings, UserView>(query, (user, us) =>
                    {
                        user.Socials = us;
                        return user;
                    }, splitOn: "id");
                    var result = res.FirstOrDefault();
                    param.Add("@UserID", result.ID);
                    result.RecentPosts = await con.QueryAsync<PostReturn, UserReturn, Categories, UserSettings, PostReturn>(postsQuery, (post, u, c, us) =>
                    {
                        post.Author = u.Username;
                        post.Category = c.Name;
                        post.AuthorSocials = us;
                        return post;
                    }, param, splitOn: "id");
                    result.RecentMedia = await con.QueryAsync<PostReturn, UserReturn, Categories, UserSettings, PostReturn>(mediaQuery, (post, u, c, us) =>
                    {
                        post.Author = u.Username;
                        post.Category = c.Name;
                        post.AuthorSocials = us;
                        return post;
                    }, param, splitOn: "id");
                    result.Following = await con.QueryAsync<UserReturn, UserSettings, UserReturn>(followingQuery, (user, us) =>
                    {
                        user.Settings = us;
                        return user;
                    }, param, splitOn: "id");

                    result.Followers = await con.QueryAsync<UserReturn, UserSettings, UserReturn>(followersQuery, (user, us) =>
                    {
                        user.Settings = us;
                        return user;
                    }, param, splitOn: "id");
                    result.UserStatistics = await con.QueryFirstOrDefaultAsync<UserStatistics>(statsQuery, param);

                    return result;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<UserSettings>? GetUserSettings(int? ID, string? Username)
        {
            try
            {
                string WhereClause = $" WHERE t.userid = {ID ?? 0} OR t.userid in (select id from users u where u.username = '{Username}')";

                string query = $@"
                SELECT *
                FROM usersettings t
                {WhereClause};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<UserSettings>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<string>? UpdateEmail(int ID, string Email)
        {
            try
            {
                var access = await CheckEmail(Email, ID);
                if (!access)
                {
                    string query = $@"
                    UPDATE users
                    SET email = '{Email}'
                    WHERE id = {ID}
                    RETURNING email;";
                    using (var connection = GetConnection)
                    {
                        var res = await connection.QueryFirstOrDefaultAsync<string>(query);
                        return res;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<bool>? ChangePassword(int UserID, string currentPassword, string newPassword)
        {
            try
            {
                string query = $@"
                UPDATE users
                SET password = '{newPassword}'
                WHERE id = {UserID};";

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryAsync(query);
                    return true;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return false;
            }
        }

        public async Task<bool>? UpdateUsername(int ID, string Username)
        {
            try
            {
                string query = $@"
                UPDATE users
                SET username = '{Username}'
                WHERE id = {ID};";

                using (var connection = GetConnection)
                {
                    await connection.QueryFirstOrDefaultAsync<ProcessResult>(query);
                    return true;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return false;
            }
        }

        public async Task<string>? UpdateBio(int ID, string Bio)
        {
            try
            {
                string query = $@"
                UPDATE usersettings
                SET bio = '{Bio}'
                WHERE userid = {ID}
                RETURNING bio;";

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<string>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<UserSettings>? UpdateSocials(int ID, UserSettings entity)
        {
            try
            {
                string query = $@"
                UPDATE usersettings
                SET facebook = '{entity.Facebook}',
                instagram = '{entity.Instagram}',
                linkedin = '{entity.Linkedin}',
                twitter = '{entity.Twitter}',
                youtube = '{entity.YouTube}',
                personal = '{entity.Personal}'
                WHERE userid = {ID}
                RETURNING facebook, instagram, linkedin, twitter, youtube, personal;";

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<UserSettings>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
        public async Task<string>? ManageAvatar(string path, int userID)
        {
            try
            {
                string query = $@"
                UPDATE usersettings
                SET picture = '{path}'
                WHERE userid = {userID}
                RETURNING picture;";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<string>(query);
                    if (res != null)
                    {
                        return path;
                    };
                    return null;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }

        }

        public async Task<bool?> ManageFollow(int TargetID, int UserID)
        {
            try
            {
                var ID = await GetUserFunctionID(TargetID, UserID, true);
                string query = "";
                if (!ID.HasValue)
                {
                    query = $@"
                INSERT INTO userfollowjunction (id, followerid, followedid)
	 	                VALUES (default, {UserID}, {TargetID})
                ON CONFLICT (id, followerid, followedid) DO NOTHING 
                RETURNING True;";
                }
                else
                {
                    query = $@"delete from userfollowjunction where followerid = {UserID} and followedid = {TargetID} RETURNING False;";
                }

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<bool>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<bool?> ManageBlock(int TargetID, int UserID)
        {
            try
            {
                var ID = await GetUserFunctionID(TargetID, UserID, false);
                string query = "";
                if (!ID.HasValue)
                {
                    query = $@"
                INSERT INTO userblockjunction (id, blockerid, blockedid)
	 	                VALUES (default, {UserID}, {TargetID})
                ON CONFLICT (id, blockerid, blockedid) DO NOTHING 
                RETURNING True;";
                }
                else
                {
                    query = $@"delete from userblockjunction where blockerid = {UserID} and blockedid = {TargetID} RETURNING False;";
                }

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<bool>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<int?> GetUserFunctionID(int TargetID, int UserID, bool function)
        {
            try
            {
                string query = "";
                if (function)
                {
                    query = $@"Select id from userfollowjunction ufj where ufj.followerid = {UserID} and ufj.followedid = {TargetID};";
                }
                else
                {
                    query = $@"Select id from userblockjunction ubj where ubj.blockerid = {UserID} and ubj.blockedid = {TargetID};";
                }

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<int?>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
        public async Task<IEnumerable<UserReturn>>? GetRandomUsers()
        {
            try
            {
                string query = $@"
                SELECT t.id as uid, t.username, us.* 
                FROM users t 
                left join usersettings us on us.userid = t.id 
                WHERE t.isactive = true 
                ORDER BY random() limit 4;";

                using (var con = GetConnection)
                {
                    var result = await con.QueryAsync<UserReturn, UserSettings, UserReturn>(query, (user, us) =>
                    {
                        user.Settings = us;
                        return user;
                    }, splitOn: "id");
                    return result;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
    }
}