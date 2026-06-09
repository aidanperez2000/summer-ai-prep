using System.Text;
using System.Text.Json;
using ToneLens.Api.Models;

namespace ToneLens.Api.Services
{
    public class OllamaToneAnalysisService : IToneAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaToneAnalysisService> _logger;

        public OllamaToneAnalysisService(HttpClient httpClient, ILogger<OllamaToneAnalysisService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Analyzes the tone of the provided text by sending a prompt to the Ollama API and parsing the response into signals, interpretations, and ambiguities.
        /// </summary>
        /// <param name="request">The request containing the text and context for tone analysis.</param>
        /// <returns>An <see cref="AnalyzeToneResponse"/> containing the analyzed tone information. If the analysis fails, a fallback response is returned.</returns>
        public async Task<AnalyzeToneResponse> AnalyzeToneAsync(AnalyzeToneRequest request, CancellationToken cancellationToken = default)
        {
            var prompt = BuildPrompt(request);
            var ollamaRequest = new OllamaGenerateRequest
            {
                Model = "qwen3",
                Prompt = prompt,
                Stream = false
            };

            var jsonRequest = JsonSerializer.Serialize(
                 ollamaRequest,
                 new JsonSerializerOptions(JsonSerializerDefaults.Web));

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await _httpClient.PostAsync("/api/generate", new StringContent(jsonRequest, Encoding.UTF8, "application/json"), cancellationToken);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Ollama request timed out before completion.");
                return BuildFallbackResponse(request);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to call Ollama API.");
                return BuildFallbackResponse(request);
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama API request failed with status code: {StatusCode}", httpResponse.StatusCode);
                return BuildFallbackResponse(request);
            }

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (ollamaResponse == null || string.IsNullOrEmpty(ollamaResponse.Response))
            {
                _logger.LogWarning("Ollama API response was empty or could not be deserialized.");
                return BuildFallbackResponse(request);
            }

            return TryParseToneResponse(ollamaResponse.Response, request);
        }

        /// <summary>
        /// Builds a prompt for the Ollama API based on the provided tone analysis request, including the text to analyze and any relevant context. The prompt instructs the model to return structured information about signals, interpretations, and ambiguities in the tone of the text.
        /// </summary>
        /// <param name="request">The request containing the text and context for tone analysis.</param>
        /// <returns>A string containing the prompt to be sent to the Ollama API.</returns>
        public static string BuildPrompt(AnalyzeToneRequest request)
        {
            return $$"""
                You are ToneLens, an AI assistant that analyzes the tone of text messages. Your task is to identify the tone of the provided text and return three key pieces of information:
                1. Signals: Specific words, phrases, or patterns in the text that indicate a particular tone. For each signal, provide a name, strength (0 to 1), and a brief explanation of why it was identified.
                2. Interpretations: A concise interpretation of the overall tone of the text, along with a confidence score (0 to 1) and reasoning for the interpretation.
                3. Ambiguities: Any aspects of the text that could lead to multiple interpretations or uncertainty in the tone analysis.

                Analyze the following text and provide your response in a structured format:
                Text: "{{request.Text}}"
                Conversation Context: "{{request.ConversationContext ?? "None"}}"
                Relationship Type: "{{request.RelationshipType ?? "None"}}"

                Please format your response as follows:
                {
                    "signals": [
                        {
                            "name": "SignalName",
                            "strength": 0.8,
                            "explanation": "Explanation of why this signal was identified."
                        }
                    ],
                    "interpretations": [
                        {
                            "interpretationText": "Overall tone interpretation.",
                            "confidenceScore": 0.85,
                            "reasoning": "Reasoning for the interpretation."
                        }
                    ],
                    "ambiguities": [
                        "Description of any ambiguities in the analysis."
                    ]
                }
            """;
        }

        /// <summary>
        /// Builds a fallback response for tone analysis when the Ollama API fails to provide a valid response. The fallback response includes a neutral signal and interpretation, along with an ambiguity indicating that the tone could be interpreted in multiple ways due to the lack of strong indicators.
        /// </summary>
        /// <param name="request">The request containing the text and context for tone analysis.</param>
        /// <returns>An <see cref="AnalyzeToneResponse"/> containing the fallback tone analysis information.</returns>
        private static AnalyzeToneResponse BuildFallbackResponse(AnalyzeToneRequest request)
        {
            return new AnalyzeToneResponse
            {
                Signals = new List<Signal>
                {
                    new Signal { Name = "Neutral", Strength = 0.5, Explanation = "The tone is neutral with no strong indicators." }
                },
                Interpretations = new List<Interpretation>
                {
                    new Interpretation { InterpretationText = "The tone is neutral.", ConfidenceScore = 0.5, Reasoning = "Based on the lack of strong positive or negative signals." }
                },
                Ambiguities = new List<string>
                {
                    "The tone could also be interpreted as positive or negative due to some ambiguous language."
                }
            };
        }

        /// <summary>
        /// Attempts to parse the model response into an <see cref="AnalyzeToneResponse"/>. If parsing fails, a fallback response is returned.
        /// </summary>
        /// <param name="modelText"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private AnalyzeToneResponse TryParseToneResponse(string modelText, AnalyzeToneRequest request)
        {
            try
            {
                var cleanedJson = ExtractJsonFromModelResponse(modelText);
                var response = JsonSerializer.Deserialize<AnalyzeToneResponse>(cleanedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (response != null)
                {
                    return response;
                }
                _logger.LogWarning("Failed to parse model response into AnalyzeToneResponse.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error while trying to parse model response.");
            }

            return BuildFallbackResponse(request);
        }

        /// <summary>
        /// Extracts the JSON portion from the model response text. This is necessary because the model may include additional text or formatting around the JSON, and we need to isolate the JSON to parse it correctly.
        /// </summary>
        /// <param name="modelText">The text returned by the model, which may contain JSON and other content.</param>
        /// <returns>A string containing only the JSON portion of the model response.</returns>
        private static string ExtractJsonFromModelResponse(string modelText)
        {
            var jsonStart = modelText.IndexOf('{');
            var jsonEnd = modelText.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                return modelText.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            return string.Empty;
        }
    }
}