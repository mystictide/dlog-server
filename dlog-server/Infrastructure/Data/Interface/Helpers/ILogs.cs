using dlog.server.Infrasructure.Models.Helpers;

namespace dlog.server.Infrastructure.Data.Interface.Helpers
{
    public interface ILogs
    {
        Task<int> Add(Logs entity);
    }
}
