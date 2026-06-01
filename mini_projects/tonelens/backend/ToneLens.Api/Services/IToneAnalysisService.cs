using ToneLens.Api.Models;

namespace ToneLens.Api.Services
{
    public interface IToneAnalysisService
    {
        AnalyzeToneResponse AnalyzeTone(AnalyzeToneRequest request);
    }
}