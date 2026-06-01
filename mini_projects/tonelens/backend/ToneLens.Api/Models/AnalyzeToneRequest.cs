namespace ToneLens.Api.Models
{
    public class AnalyzeToneRequest
    {
        public string Text { get; set; } = string.Empty;
        public string? ConversationContext { get; set; }
        public string? RelationshipType { get; set; }
    }
}