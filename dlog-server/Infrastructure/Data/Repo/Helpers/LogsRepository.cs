using System.Diagnostics;
using Dapper.Contrib.Extensions;
using dlog.server.Infrasructure.Models.Helpers;
using dlog.server.Infrastructure.Models.Helpers;
using dlog.server.Infrastructure.Data.Interface.Helpers;

namespace dlog.server.Infrastructure.Data.Repo.Helpers
{
    public class LogsRepository : AppSettings, ILogs
    {
        public async Task<int> Add(Logs entity)
        {
            try
            {
                using (var con = GetConnection)
                {
                    return await con.InsertAsync(entity);
                }
            }
            catch (Exception exception)
            {
                string a = exception.Message;
                return 0;
            }
        }

        public async Task<bool> CreateLog(Exception ex, int UserId = 0)
        {
            try
            {
                var st = new StackTrace(ex, true);
                if (st != null)
                {
                    st.GetFrames().Where(k => k.GetFileLineNumber() > 0).ToList().ForEach(async k =>
                    {
                        await new LogsRepository().Add(new Logs()
                        {
                            CreatedDate = DateTime.Now,
                            UserID = UserId,
                            Message = ex.Message,
                            Source = ex.Source + " | " + k,
                            Line = k.GetFileLineNumber()
                        });
                    });
                }
                else
                {
                    await new LogsRepository().Add(new Logs()
                    {
                        CreatedDate = DateTime.Now,
                        UserID = UserId,
                        Message = ex.Message,
                        Source = ex.Source,
                        Line = 0
                    });
                }
            }
            catch (Exception exception)
            {
                await new LogsRepository().Add(new Logs()
                {
                    CreatedDate = DateTime.Now,
                    UserID = 0,
                    Message = exception.Message,
                    Source = exception.Source + " - Error logging",
                    Line = 0
                });
            }
            return true;
        }
    }
}
