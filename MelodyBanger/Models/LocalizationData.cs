namespace MelodyBanger.Models
{
    public class LocalizationData
    {
        public List<string> Genres { get; set; } = new();
        public List<string> TitleAdjectives { get; set; } = new();
        public List<string> TitleNouns { get; set; } = new();
        public List<string> TitleVerbs { get; set; } = new();
        public List<string> TitlePatterns { get; set; } = new();
        public List<string> AlbumPatterns { get; set; } = new();
        public List<string> ArtistFirstNames { get; set; } = new();
        public List<string> ArtistLastNames { get; set; } = new();
        public List<string> BandNameParts { get; set; } = new();
        public List<string> ReviewPhrases { get; set; } = new();
        public List<string> ArtistPrefixes { get; set; } = new();
        public List<string> LyricsLines { get; set; } = new();
    }
}
