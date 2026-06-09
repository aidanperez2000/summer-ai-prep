using ToneLens.Api.Models;

namespace ToneLens.Api.Services
{
    public interface IToneAnalysisService
    {
        Task<AnalyzeToneResponse> AnalyzeToneAsync(AnalyzeToneRequest request, CancellationToken cancellationToken = default);
    }
}