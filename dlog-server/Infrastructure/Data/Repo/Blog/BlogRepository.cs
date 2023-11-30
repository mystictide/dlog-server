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
                string WhereClause = "";
                if (filter.Keyword != null && filter.Keyword != "")
                {
                    WhereClause = $@" WHERE t.title ILIKE '%{filter.Keyword}%'";
                }
                if (filter.CategoryName != null && filter.CategoryName != "")
                {
                    WhereClause = $@" WHERE t.categoryid in (SELECT id from categories where name = '{filter.CategoryName}')
                    AND t.title ILIKE '%{filter.Keyword}%'";
                }
                if (filter.CategoryID > 0)
                {
                    WhereClause = $@"WHERE t.categoryid = {filter.CategoryID}
                    AND t.title ILIKE '%{filter.Keyword}%'";
                }

                string query_count = $@"Select Count(t.id) from posts t {WhereClause}";

                using (var con = GetConnection)
                {
                    result.totalItems = await con.QueryFirstOrDefaultAsync<int>(query_count);
                    request.filter.pager = new Page(result.totalItems, request.filter.pageSize, request.filter.page);
                    string query = $@"
                    SELECT * FROM posts t
                    {WhereClause}
                    ORDER BY t.id {filter.SortBy}
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
                string WhereClause = $" WHERE t.id = {ID ?? 0} OR (t.title ilike '%{Title}%')";

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
                string WhereClause = $" WHERE t.id = {ID ?? 0} OR (t.title ilike '%{Title}%')";

                string query = $@"
                SELECT t.*, c.id, c.name, u.id, u.username, u2.*
                FROM posts t
                left join categories c on c.id = t.categoryid
                left join users u on u.id = t.userid
                left join usersettings u2 on u2.userid = t.userid
                {WhereClause};";

                using (var con = GetConnection)
                {
                    var res = await con.QueryAsync<PostReturn, Categories, UserReturn, UserSettings, PostReturn>(query, (post, c, u, us) => {
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
                string WhereClause = $"WHERE t.date > current_date - interval '7 days'";

                string query = $@"
                SELECT *,
                (select name from categories c where c.id = t.categoryid) as Category,
                (select username from users u where u.id = t.userid) as Author
                FROM posts t
                {WhereClause}
                order by t.date desc limit 5;";

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
                INSERT INTO posts (id, userid, categoryid, title, body, date, isactive)
	 	                VALUES (
                {identity}, {UserID}, {entity.CategoryID}, '{entity.Title}', '{entity.Body}', current_timestamp, true)
                ON CONFLICT (id) DO UPDATE 
                SET title = '
                {entity.Title}',
                       body = '
                {entity.Body}',
                       categoryid = 
                {entity.CategoryID},
                        updatedate = current_timestamp
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
