namespace MelodyBanger.Models
{
    public class GenerationParams
    {
        public long Seed { get; set; }
        public string Lang { get; set; } = "en";
        public int Page { get; set; } = 1;
        public double Likes { get; set; } = 0;
        public int PageSize { get; set; } = 10;
    }
}
