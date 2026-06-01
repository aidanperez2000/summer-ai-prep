namespace ToneLens.Api.Models
{
    public class Interpretation
    {
        public string InterpretationText { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }
}