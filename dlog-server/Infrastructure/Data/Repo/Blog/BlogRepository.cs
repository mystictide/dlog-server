using Dapper;
using dlog_server.Infrastructure.Models.Blog;
using dlog.server.Infrasructure.Models.Helpers;
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
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Posts>? Get(int? ID, string? Title)
        {
            try
            {
                string WhereClause = $" WHERE t.id = {ID ?? 0} OR (t.title like '%{Title}%')";

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
                LogsRepository.CreateLog(ex);
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
                LogsRepository.CreateLog(ex);
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
                INSERT INTO posts (id, userid, categoryid, title, body, isactive)
	 	                VALUES (
                {identity}, '{entity.UserID}', '{entity.CategoryID}', '{entity.Title}', '{entity.Body}', '{entity.IsActive}')
                ON CONFLICT (id) DO UPDATE 
                SET title = '
                {entity.Title}',
                       body = '
                {entity.Body}',
                       categoryid = '
                {entity.CategoryID}'
                RETURNING *;";

                using (var connection = GetConnection)
                {
                    var res = await connection.QueryFirstOrDefaultAsync<Posts>(query);
                    return res;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
