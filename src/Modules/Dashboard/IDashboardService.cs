using System.Threading;
using System.Threading.Tasks;

namespace ItSupportServer.src.Modules.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(
            DashboardSummaryQueryDto query,
            string roleScope,
            CancellationToken cancellationToken = default);
    }
}
