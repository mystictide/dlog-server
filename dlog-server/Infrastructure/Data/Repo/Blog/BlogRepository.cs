using Dapper;
using dlog_server.Infrastructure.Models.Blog;
using dlog_server.Infrastructure.Models.Users;
using dlog.server.Infrasructure.Models.Helpers;
using dlog.server.Infrasructure.Models.Returns;
using dlog.server.Infrastructure.Models.Helpers;
using dlog.server.Infrastructure.Data.Repo.Helpers;
using dlog_server.Infrastructure.Data.Interface.Blog;

namespace dlog_server.Infrastructure.Data.Repo.Blog
{
    public class BlogRepository : AppSettings, IBlog
    {
        public async Task<FilteredList<PostReturn>> FilterPosts(Filter filter)
        {
            try
            {
                var filterModel = new PostReturn();
                FilteredList<PostReturn> request = new FilteredList<PostReturn>()
                {
                    filter = filter,
                    filterModel = filterModel,
                };
                FilteredList<PostReturn> result = new FilteredList<PostReturn>();
                string kw = "''";
                string cat = "";
                if (filter.Keyword != null)
                {
                    kw = $@"'%{filter.Keyword}%'";
                }
                if (filter.Category == 0)
                {
                    cat = "notnull ";
                }
                else
                {
                    cat = $@"= {filter.Category} ";
                }

                string WhereClause = $@"WHERE t.ismedia = {filter.IsMedia} 
                and t.title ilike {kw} or t.categoryid {cat}";

                if (filter.IsAdvanced)
                {
                    WhereClause = $@"WHERE t.ismedia = {filter.IsMedia} 
                and t.title ilike {kw}
                and t.categoryid {cat}";
                }
                if (filter.Author != null && filter.IsAdvanced)
                {
                    WhereClause = $@"WHERE t.ismedia = {filter.IsMedia} 
                and t.categoryid {cat} 
                and t.userid in (select id from users u where u.username ilike '{filter.Author}')";
                }
                string query_count = $@"Select Count(t.id) from posts t {WhereClause}";

                using (var con = GetConnection)
                {
                    result.totalItems = await con.QueryFirstOrDefaultAsync<int>(query_count);
                    request.filter.pager = new Page(result.totalItems, request.filter.pageSize, request.filter.page);
                    string query = $@"
                    SELECT *,
                    (select name from categories c where c.id = t.categoryid) as Category,
                    (select username from users us where us.id = t.userid) as Author
                    FROM posts t
                    {WhereClause}
                    order by COALESCE(t.updatedate, t.date) {filter.SortBy}
                    OFFSET {request.filter.pager.StartIndex} ROWS
                    FETCH NEXT {request.filter.pageSize} ROWS ONLY";
                    result.data = await con.QueryAsync<PostReturn>(query);
                    result.filter = request.filter;
                    result.filterModel = request.filterModel;
                    return result;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<Posts>? Get(int? ID, string? Title)
        {
            try
            {
                string WhereClause;
                if (Title != null)
                {
                    WhereClause = $" WHERE t.id = {ID ?? 0} AND (t.title ilike '{Title}')";
                }
                else
                {
                    WhereClause = $" WHERE t.id = {ID ?? 0}";
                }

                string query = $@"
                SELECT *
                FROM posts t
                {WhereClause};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<Posts>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
        public async Task<Comments>? GetComment(int ID)
        {
            try
            {
                string query = $@"
                SELECT * FROM comments t WHERE t.id = {ID};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<Comments>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<PostReturn>? GetView(int? ID, string? Title)
        {
            try
            {
                string WhereClause = $" WHERE t.id = {ID ?? 0} AND (t.title ilike '{Title}')";

                string query = $@"
                SELECT t.*, c.id, c.name, u.id, u.username, u2.*
                FROM posts t
                left join categories c on c.id = t.categoryid
                left join users u on u.id = t.userid
                left join usersettings u2 on u2.userid = t.userid
                {WhereClause};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryAsync<PostReturn, Categories, UserReturn, UserSettings, PostReturn>(query, (post, c, u, us) =>
                    {
                        post.Author = u.Username;
                        post.Category = c.Name;
                        post.AuthorSocials = us;
                        return post;
                    }, splitOn: "id");

                    return res.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
        public async Task<IEnumerable<PostReturn>>? GetRecentPosts()
        {
            try
            {
                string WhereClause = $"WHERE ismedia = false AND t.date > current_date - interval '7 days' OR t.updatedate > current_date - interval '7 days'";

                string query = $@"
                SELECT *,
                (select name from categories c where c.id = t.categoryid) as Category,
                (select username from users u where u.id = t.userid) as Author
                FROM posts t
                {WhereClause}
                order by COALESCE(t.updatedate, t.date) desc limit 5;";

                using (var con = GetConnection)
                {
                    var res = await con.QueryAsync<PostReturn>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
        public async Task<bool>? ToggleVisibility(Posts entity)
        {
            try
            {
                string query = $@"
                UPDATE posts
                SET isactive = {entity.IsActive}
                WHERE id = {entity.ID};
                RETURNING isactive;";

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<bool>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return false;
            }
        }
        public async Task<Posts>? ManagePost(int UserID, Posts entity)
        {
            try
            {
                dynamic identity = entity.ID.HasValue ? entity.ID.Value : "default";

                if (entity.Title.Contains("'"))
                {
                    entity.Title = entity.Title.Replace("'", "''");
                }
                if (entity.Body.Contains("'"))
                {
                    entity.Body = entity.Body.Replace("'", "''");
                }

                string query = $@"
                SET datestyle = dmy;
                INSERT INTO posts (id, userid, categoryid, title, body, date, isactive, ismedia)
	 	                VALUES (
                {identity}, {UserID}, {entity.CategoryID}, '{entity.Title}', '{entity.Body}', current_timestamp, true, {entity.IsMedia})
                ON CONFLICT (id) DO UPDATE 
                SET title = '{entity.Title}',
                       body = '{entity.Body}',
                       categoryid = {entity.CategoryID},
                       updatedate = current_timestamp,
                       ismedia = {entity.IsMedia}
                RETURNING *;";

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<Posts>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<Comments>? ManageComment(int UserID, Comments entity)
        {
            try
            {
                dynamic identity = entity.ID.HasValue ? entity.ID.Value : "default";

                if (entity.Body.Contains("'"))
                {
                    entity.Body = entity.Body.Replace("'", "''");
                }

                string query = $@"
                SET datestyle = dmy;
                INSERT INTO posts (id, postid, userid, body, date)
	 	                VALUES (
                {identity}, {entity.PostID}, {UserID}, '{entity.Body}', '{entity.Date}', current_timestamp)
                ON CONFLICT (id) DO UPDATE 
                SET body = '{entity.Body}',
                       updatedate = current_timestamp
                RETURNING *;";

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<Comments>(query);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<bool?> ManagePostVote(int? ID, int UserID, int PostID, bool? vote)
        {
            try
            {
                dynamic identity = ID.HasValue ? ID.Value : "default";
                string query = "";
                if (vote.HasValue)
                {
                    query = $@"
                INSERT INTO postvotesjunction (id, postid, userid, vote)
	 	                VALUES (
                {identity}, {PostID}, {UserID}, '{vote}')
                ON CONFLICT (id, postid, userid) DO UPDATE 
                SET vote = {vote}
                RETURNING vote;";
                }
                else
                {
                    query = $@"delete from postvotesjunction where id = {ID} and postid = {PostID} and userid = {UserID}";
                }

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<bool>(query);
                    if (!vote.HasValue)
                    {
                        return null;
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public async Task<bool?> ManageCommentVote(int? ID, int UserID, int CommentID, bool? vote)
        {
            try
            {
                dynamic identity = ID.HasValue ? ID.Value : "default";
                string query = "";
                if (vote.HasValue)
                {
                    query = $@"
                INSERT INTO commentvotesjunction (id, commentid, userid, vote)
	 	                VALUES (
                {identity}, {CommentID}, {UserID}, '{vote}')
                ON CONFLICT (id, commentid, userid) DO UPDATE 
                SET vote = {vote}
                RETURNING vote;";
                }
                else
                {
                    query = $@"delete from commentvotesjunction where id = {ID} and commentid = {CommentID} and userid = {UserID}";
                }

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<bool>(query);
                    if (!vote.HasValue)
                    {
                        return null;
                    }
                    return res;
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
