namespace ToneLens.Api.Models
{
    public class Signal
    {
        public string Name { get; set; } = string.Empty;
        public double Strength { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}