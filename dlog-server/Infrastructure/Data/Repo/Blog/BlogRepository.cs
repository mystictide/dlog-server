using Dapper;
using dlog_server.Infrastructure.Models.Blog;
using dlog_server.Infrastructure.Models.Users;
using dlog.server.Infrasructure.Models.Helpers;
using dlog.server.Infrasructure.Models.Returns;
using dlog.server.Infrastructure.Models.Helpers;
using dlog_server.Infrastructure.Models.Returns;
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
        public async Task<PostReturn>? GetView(int? ID, string? Title, int? UserID)
        {
            try
            {
                string WhereClause = $"WHERE t.id = {ID ?? 0} AND (t.title ilike '{Title}')";

                string query = $@"
                SELECT t.*, p.vote as UserVote, c.id, c.name, u.id, u.username, u2.*
                FROM posts t
                left join postvotesjunction p on p.userid = {UserID ?? 0}
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
                    var result = res.FirstOrDefault();
                    var statsq = $@"select
                    (SELECT count(t.id) FROM postvotesjunction t where t.postid = {result.ID ?? 0} and t.vote = true) as Upvotes,
                    (SELECT count(t.id) FROM postvotesjunction t where t.postid = {result.ID ?? 0} and t.vote = false) as Downvotes
                    from posts p where p.id = {result.ID ?? 0};";
                    result.Votes = await con.QueryFirstOrDefaultAsync<PostStatistics>(statsq);
                    return result;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
        public async Task<FilteredList<CommentReturn>>? FilterComments(Filter filter, int? UserID)
        {
            try
            {
                var filterModel = new CommentReturn();
                FilteredList<CommentReturn> request = new FilteredList<CommentReturn>()
                {
                    filter = filter,
                    filterModel = filterModel,
                };
                FilteredList<CommentReturn> result = new FilteredList<CommentReturn>();

                string WhereClause = $@"WHERE t.postid = {filter.ID}";
                string query_count = $@"Select Count(t.id) from comments t {WhereClause}";

                using (var con = GetConnection)
                {
                    result.totalItems = await con.QueryFirstOrDefaultAsync<int>(query_count);
                    request.filter.pager = new Page(result.totalItems, request.filter.pageSize, request.filter.page);
                    string query = $@"
                    SELECT t.*, c.vote as UserVote, u.id, u.username, cvu.id, count(cvu.id) Upvotes, count(cvd.id) Downvotes, u2.*
                    FROM comments t
                    left join commentvotesjunction c on c.userid = {UserID ?? 0} and c.commentid = t.id
                    left join users u on u.id = t.userid
                    left join commentvotesjunction cvu on cvu.commentid = t.id and cvu.vote = true
                    left join commentvotesjunction cvd on cvd.commentid = t.id and cvd.vote = false  
                    left join usersettings u2 on u2.userid = t.userid
                    {WhereClause}
                    group by t.id, c.vote, u.id, cvu.id, u2.id
                    order by t.date {filter.SortBy}
                    OFFSET {request.filter.pager.StartIndex} ROWS
                    FETCH NEXT {request.filter.pageSize} ROWS ONLY";

                    result.data = await con.QueryAsync<CommentReturn, UserReturn, CommentStatistics, UserSettings, CommentReturn>(query, (comment, u, cs, us) =>
                    {
                        comment.Author = u.Username;
                        comment.AuthorSocials = us;
                        comment.Votes = cs;
                        return comment;
                    }, splitOn: "id");

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
        public async Task<IEnumerable<PostReturn>>? GetRecentPosts(bool isMedia)
        {
            try
            {
                //string WhereClause = $"WHERE ismedia = {isMedia} AND t.date > current_date - interval '7 days' OR t.updatedate > current_date - interval '7 days'";

                string query = $@"
                SELECT t.*, u.id, u.username, c.*, u2.id, u2.picture
                FROM posts t
                left join users u on u.id = t.userid
                left join categories c on c.id = t.categoryid
                left join usersettings u2 on u2.userid = t.userid
                WHERE t.ismedia = {isMedia} and t.isactive = true
                order by COALESCE(t.updatedate, t.date) desc limit {(isMedia ? 8 : 6)};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryAsync<PostReturn, UserReturn, Categories, UserSettings, PostReturn>(query, (post, u, c, us) =>
                     {
                         post.Author = u.Username;
                         post.Category = c.Name;
                         post.AuthorSocials = us;
                         return post;
                     }, splitOn: "id");
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
        public async Task<PostStatistics>? GetPostStatistics(int ID)
        {
            try
            {
                var statsq = $@"select
                    (SELECT count(t.id) FROM postvotesjunction t where t.postid = {ID} and t.vote = true) as Upvotes,
                    (SELECT count(t.id) FROM postvotesjunction t where t.postid = {ID} and t.vote = false) as Downvotes
                    from posts p where p.id = {ID};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<PostStatistics>(statsq);
                    return res;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
        public async Task<CommentStatistics>? GetCommentStatistics(int ID)
        {
            try
            {
                var statsq = $@"select
                    (SELECT count(t.id) FROM commentvotesjunction t where t.commentid = {ID} and t.vote = true) as Upvotes,
                    (SELECT count(t.id) FROM commentvotesjunction t where t.commentid = {ID} and t.vote = false) as Downvotes
                    from comments p where p.id = {ID};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryFirstOrDefaultAsync<CommentStatistics>(statsq);
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
                WHERE id = {entity.ID} 
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
                INSERT INTO comments (id, postid, userid, body, date)
	 	                VALUES (
                {identity}, {entity.PostID}, {UserID}, '{entity.Body}', current_timestamp)
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

        public async Task<bool?> ManagePostVote(int UserID, int PostID, bool? vote)
        {
            try
            {
                string query = "";
                if (vote.HasValue)
                {
                    query = $@"
                INSERT INTO postvotesjunction (id, postid, userid, vote)
	 	                VALUES (
                default, {PostID}, {UserID}, '{vote}')
                ON CONFLICT (postid, userid) DO UPDATE 
                SET vote = {vote}
                RETURNING vote;";
                }
                else
                {
                    query = $@"delete from postvotesjunction where postid = {PostID} and userid = {UserID}";
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

        public async Task<bool?> ManageCommentVote(int UserID, int CommentID, bool? vote)
        {
            try
            {
                string query = "";
                if (vote.HasValue)
                {
                    query = $@"
                INSERT INTO commentvotesjunction (id, commentid, userid, vote)
	 	                VALUES (
                default, {CommentID}, {UserID}, '{vote}')
                ON CONFLICT (commentid, userid) DO UPDATE 
                SET vote = {vote}
                RETURNING vote;";
                }
                else
                {
                    query = $@"delete from commentvotesjunction where commentid = {CommentID} and userid = {UserID}";
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
