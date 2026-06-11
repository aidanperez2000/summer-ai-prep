namespace ToneLens.Api.Models
{
    public class SuggestedRewrite
    {
        public string Tone { get; set; } = string.Empty;

        public string RewrittenText { get; set; } = string.Empty;

        public string Explanation { get; set; } = string.Empty;
    }
}