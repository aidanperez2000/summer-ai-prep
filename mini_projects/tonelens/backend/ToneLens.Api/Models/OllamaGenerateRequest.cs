namespace ToneLens.Api.Models
{
    public class OllamaGenerateRequest
    {
        public string Model { get; set; } = "qwen3";
        public string Prompt { get; set; } = string.Empty;
        public bool Stream { get; set; } = false;
    }
}