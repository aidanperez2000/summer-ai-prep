namespace ToneLens.Api.Models
{
    public class AnalyzeToneResponse
    {
        public List<Signal> Signals { get; set; } = new();
        public List<Interpretation> Interpretations { get; set; } = new();
        public List<string>? Ambiguities { get; set; }
    }
}