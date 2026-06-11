using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
                You are ToneLens, an assistant that analyzes communication tone.

                Analyze the message and return ONLY valid JSON.

                Message:
                {{request.Text}}

                Conversation context:
                {{request.ConversationContext}}

                Relationship type:
                {{request.RelationshipType}}

                Return JSON in this exact shape:

                {
                "signals": [
                    {
                    "name": "string",
                    "strength": 0.0,
                    "explanation": "string"
                    }
                ],
                "interpretations": [
                    {
                    "interpretationText": "string",
                    "confidenceScore": 0.0,
                    "reasoning": "string"
                    }
                ],
                "ambiguities": [
                    "string"
                ],
                "suggestedRewrites": [
                    {
                    "tone": "Warmer",
                    "rewrittenText": "string",
                    "explanation": "string"
                    },
                    {
                    "tone": "More neutral",
                    "rewrittenText": "string",
                    "explanation": "string"
                    },
                    {
                    "tone": "Clearer",
                    "rewrittenText": "string",
                    "explanation": "string"
                    }
                ]
                }

                Rules:
                - Keep suggested rewrites close to the user's original meaning.
                - Do not make the message longer unless needed.
                - Generate 3 suggested rewrites.
                - Strength and confidenceScore must be between 0 and 1.
                - Return JSON only.
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
                },
                SuggestedRewrites = new List<SuggestedRewrite>
                {
                    new SuggestedRewrite { Tone = "Warmer", RewrittenText = request.Text, Explanation = "The tone is neutral, but a warmer tone could be more engaging." },
                    new SuggestedRewrite { Tone = "More neutral", RewrittenText = request.Text, Explanation = "The tone is already neutral, maintaining this tone ensures clarity." },
                    new SuggestedRewrite { Tone = "Clearer", RewrittenText = request.Text, Explanation = "The tone is neutral, but a clearer tone could improve understanding." }
                }
            };
        }

        /// <summary>
        /// Attempts to parse the model response into an <see cref="AnalyzeToneResponse"/>. If parsing fails, a fallback response is returned.
        /// </summary>
        /// <param name="modelText">The text returned by the model, which may contain JSON and other content.</param>
        /// <param name="request">The request containing the text and context for tone analysis.</param>
        /// <returns>An <see cref="AnalyzeToneResponse"/> containing the parsed or fallback tone analysis information.</returns>
        private AnalyzeToneResponse TryParseToneResponse(string modelText, AnalyzeToneRequest request)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            try
            {
                var cleanedJson = ExtractJsonFromModelResponse(modelText);
                if (string.IsNullOrWhiteSpace(cleanedJson))
                {
                    _logger.LogWarning("No JSON object could be extracted from model response.");
                    return BuildFallbackResponse(request);
                }

                var response = JsonSerializer.Deserialize<AnalyzeToneResponse>(cleanedJson, jsonOptions);
                if (response != null)
                {
                    return NormalizeResponse(response, request);
                }
                _logger.LogWarning("Failed to parse model response into AnalyzeToneResponse.");
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "JSON parsing error while trying to parse model response.");
            }

            return BuildFallbackResponse(request);
        }

        /// <summary>
        /// Ensures the parsed model response has required collections and exactly three suggested rewrites for the UI.
        /// </summary>
        private static AnalyzeToneResponse NormalizeResponse(AnalyzeToneResponse response, AnalyzeToneRequest request)
        {
            response.Signals ??= new List<Signal>();
            response.Interpretations ??= new List<Interpretation>();
            response.Ambiguities ??= new List<string>();
            response.SuggestedRewrites ??= new List<SuggestedRewrite>();

            if (response.SuggestedRewrites.Count > 3)
            {
                response.SuggestedRewrites = response.SuggestedRewrites.Take(3).ToList();
            }

            while (response.SuggestedRewrites.Count < 3)
            {
                var tone = response.SuggestedRewrites.Count switch
                {
                    0 => "Warmer",
                    1 => "More neutral",
                    _ => "Clearer"
                };

                response.SuggestedRewrites.Add(new SuggestedRewrite
                {
                    Tone = tone,
                    RewrittenText = request.Text,
                    Explanation = "Fallback rewrite added because model output was incomplete."
                });
            }

            return response;
        }

        /// <summary>
        /// Extracts the JSON portion from the model response text. This is necessary because the model may include additional text or formatting around the JSON, and we need to isolate the JSON to parse it correctly.
        /// </summary>
        /// <param name="modelText">The text returned by the model, which may contain JSON and other content.</param>
        /// <returns>A string containing only the JSON portion of the model response.</returns>
        private static string ExtractJsonFromModelResponse(string modelText)
        {
            if (string.IsNullOrWhiteSpace(modelText))
            {
                return string.Empty;
            }

            var withoutCodeFences = Regex.Replace(modelText, "^```(?:json)?|```$", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline).Trim();

            if (TryExtractFirstBalancedJsonObject(withoutCodeFences, out var extractedJson))
            {
                return extractedJson;
            }

            var jsonStart = withoutCodeFences.IndexOf('{');
            var jsonEnd = withoutCodeFences.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                return withoutCodeFences.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            return string.Empty;
        }

        private static bool TryExtractFirstBalancedJsonObject(string text, out string json)
        {
            json = string.Empty;

            var depth = 0;
            var startIndex = -1;
            var inString = false;
            var escaped = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }

                    if (ch == '\\')
                    {
                        escaped = true;
                        continue;
                    }

                    if (ch == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    continue;
                }

                if (ch == '{')
                {
                    if (depth == 0)
                    {
                        startIndex = i;
                    }

                    depth++;
                    continue;
                }

                if (ch == '}')
                {
                    if (depth == 0)
                    {
                        continue;
                    }

                    depth--;

                    if (depth == 0 && startIndex >= 0)
                    {
                        json = text.Substring(startIndex, i - startIndex + 1);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}