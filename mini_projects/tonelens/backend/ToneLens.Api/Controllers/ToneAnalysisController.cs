using Microsoft.AspNetCore.Mvc;
using ToneLens.Api.Models;
using ToneLens.Api.Services;

namespace ToneLens.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToneAnalysisController : ControllerBase
    {
        private readonly ILogger<ToneAnalysisController> _logger;

        private readonly IToneAnalysisService _toneAnalysisService;

        public ToneAnalysisController(ILogger<ToneAnalysisController> logger, IToneAnalysisService toneAnalysisService)
        {
            _logger = logger;
            _toneAnalysisService = toneAnalysisService;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<AnalyzeToneResponse>> AnalyzeTone([FromBody] AnalyzeToneRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Text))
            {
                return BadRequest(new { error = "Text is required for tone analysis." });
            }

            _logger.LogInformation("Analyzing tone for text: {Text}", request.Text);
            var response = await _toneAnalysisService.AnalyzeToneAsync(request, cancellationToken);

            return Ok(response);
        }
    }
}