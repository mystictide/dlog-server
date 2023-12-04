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
        public async Task<FilteredList<Posts>> FilterPosts(Filter filter)
        {
            try
            {
                var filterModel = new Posts();
                filter.pageSize = 16;
                FilteredList<Posts> request = new FilteredList<Posts>()
                {
                    filter = filter,
                    filterModel = filterModel,
                };
                FilteredList<Posts> result = new FilteredList<Posts>();
                string WhereClause = $@"WHERE t.ismedia = false and t.title ilike '%{filter.Keyword}%' or t.categoryid = {filter.Category} or t.userid in (select id from users u where u.username ilike '{filter.Author}')";
                string query_count = $@"Select Count(t.id) from posts t {WhereClause}";

                using (var con = GetConnection)
                {
                    result.totalItems = await con.QueryFirstOrDefaultAsync<int>(query_count);
                    request.filter.pager = new Page(result.totalItems, request.filter.pageSize, request.filter.page);
                    string query = $@"
                    SELECT * FROM posts t
                    {WhereClause}
                    order by t.updatedate, t.date {filter.SortBy}
                    OFFSET {request.filter.pager.StartIndex} ROWS
                    FETCH NEXT {request.filter.pageSize} ROWS ONLY";
                    result.data = await con.QueryAsync<Posts>(query);
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
                order by t.updatedate, t.date desc limit 5;";

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
    }
}
