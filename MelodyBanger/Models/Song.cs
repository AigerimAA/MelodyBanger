namespace MelodyBanger.Models
{
    public class Song
    {
        public int Index { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Likes { get; set; }
        public string CoverUrl { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        public string Review { get; set; } = string.Empty;
        public string Lyrics { get; set; } = string.Empty;
    }
}
