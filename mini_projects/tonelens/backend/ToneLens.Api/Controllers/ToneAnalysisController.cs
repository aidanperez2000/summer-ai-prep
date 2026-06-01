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

        /// <summary>
        /// Analyzes the tone of the provided text and returns signals, interpretations, and ambiguities.
        /// </summary>
        /// <param name="request">The request containing the text to analyze.</param>
        /// <returns>The response containing the analysis results.</returns>
        [HttpPost("analyze")]
        public ActionResult<AnalyzeToneResponse> AnalyzeTone([FromBody] AnalyzeToneRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
            {
                return BadRequest(new { error = "Text is required for tone analysis." });
            }

            _logger.LogInformation("Analyzing tone for text: {Text}", request.Text);
            var response = _toneAnalysisService.AnalyzeTone(request);

            return Ok(response);
        }
    }
}