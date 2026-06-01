using ToneLens.Api.Models;

namespace ToneLens.Api.Services
{
    public class ToneAnalysisService : IToneAnalysisService
    {
        /// <summary>
        /// Analyzes the tone of the provided text and returns signals, interpretations, and ambiguities.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AnalyzeToneResponse AnalyzeTone(AnalyzeToneRequest request)
        {
            var text = request.Text;
            var signals = new List<Signal>();
            var interpretations = new List<Interpretation>();
            var ambiguities = new List<string> 
            {
                "The tone could also be interpreted as neutral due to some ambiguous language."
            };

            if (IsPositive(text))
            {
                signals.Add(new Signal { Name = "Positive", Strength = 0.8, Explanation = "The text contains positive language." });
                interpretations.Add(new Interpretation { InterpretationText = "The tone is generally positive.", ConfidenceScore = 0.85, Reasoning = "Based on the presence of positive signals." });
            }
            else if (IsNegative(text))
            {
                signals.Add(new Signal { Name = "Negative", Strength = 0.7, Explanation = "The text contains negative language." });
                interpretations.Add(new Interpretation { InterpretationText = "The tone is generally negative.", ConfidenceScore = 0.75, Reasoning = "Based on the presence of negative signals." });
            }
            else if (IsFrustrated(text))
            {
                signals.Add(new Signal { Name = "Frustrated", Strength = 0.6, Explanation = "The text contains signs of frustration." });
                interpretations.Add(new Interpretation { InterpretationText = "The tone is frustrated.", ConfidenceScore = 0.65, Reasoning = "Based on the presence of frustrated signals." });
            }
            else
            {
                signals.Add(new Signal { Name = "Neutral", Strength = 0.5, Explanation = "The tone is neutral with no strong indicators." });
                interpretations.Add(new Interpretation { InterpretationText = "The tone is neutral.", ConfidenceScore = 0.5, Reasoning = "Based on the lack of strong positive or negative signals." });
            }
            
            if (!string.IsNullOrEmpty(request.ConversationContext))
            {
                ambiguities.Add("The context provided may influence the tone interpretation, adding some uncertainty.");
            }

            if (!string.IsNullOrEmpty(request.RelationshipType))
            {
                ambiguities.Add("The relationship context may also affect the tone, introducing additional ambiguity.");
            }

            return new AnalyzeToneResponse
            {
                Signals = signals,
                Interpretations = interpretations,
                Ambiguities = ambiguities
            };
        }

        /// <summary>
        /// Determines if the text has a positive tone based on the presence of certain keywords.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool IsPositive(string text)
        {
            var positiveWords = new List<string> { "good", "happy", "joy", "excellent", "positive" };
            return positiveWords.Any(word => text.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines if the text has a negative tone based on the presence of certain keywords.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool IsNegative(string text)
        {
            var negativeWords = new List<string> { "bad", "sad", "angry", "terrible", "negative" };
            return negativeWords.Any(word => text.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines if the text has a frustrated tone based on the presence of certain keywords.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool IsFrustrated(string text)
        {
            var frustratedWords = new List<string> { "frustrated", "annoyed", "irritated", "upset" };
            return frustratedWords.Any(word => text.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }
}