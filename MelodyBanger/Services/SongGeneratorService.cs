using System.Security.Cryptography;
using MelodyBanger.Models;

namespace MelodyBanger.Services
{
    public class SongGeneratorService
    {
        private readonly LocalizationService _localization;

        public SongGeneratorService(LocalizationService localization)
        {
            _localization = localization;
        }

        public async Task<SongPage> GeneratePage(GenerationParams p)
        {
            var locale = await _localization.GetLocalization(p.Lang);

            var songs = new List<Song>();

            for (int i = 0; i < p.PageSize; i++)
            {
                int globalIndex = (p.Page - 1) * p.PageSize + i + 1;

                var songSeed = HashCode.Combine(p.Seed, globalIndex);
                var songRandom = new Random(songSeed);  

                songs.Add(new Song
                {
                    Index = globalIndex,
                    Title = GenerateTitle(songRandom, locale),      
                    Artist = GenerateArtist(songRandom, locale),   
                    Album = GenerateAlbum(songRandom, locale),    
                    Genre = Pick(songRandom, locale.Genres),       
                    Likes = GenerateLikes(songRandom, p.Likes),  
                    Review = Pick(songRandom, locale.ReviewPhrases),
                    Lyrics = GenerateLyrics(songRandom, locale)
                });
            }

            return new SongPage
            {
                Items = songs,
                CurrentPage = p.Page,
                TotalPages = 100,
                PageSize = p.PageSize
            };
        }

        private string ReplacePatterns(string pattern, LocalizationData locale, Random randomNumber)
        {
            return pattern
                .Replace("{adj}", Pick(randomNumber, locale.TitleAdjectives))
                .Replace("{noun}", Pick(randomNumber, locale.TitleNouns))
                .Replace("{verb}", Pick(randomNumber, locale.TitleVerbs));
        }

        private string GenerateTitle(Random randomNumber, LocalizationData locale)
        {
            var pattern = Pick(randomNumber, locale.TitlePatterns);
            return ReplacePatterns(pattern, locale, randomNumber);
        }

        private string GenerateAlbum(Random randomNumber, LocalizationData locale)
        {
            var pattern = Pick(randomNumber, locale.AlbumPatterns);

            if (!pattern.Contains('{'))
                return pattern;

            return ReplacePatterns(pattern, locale, randomNumber);
        }

        private string GenerateArtist(Random randomNumber, LocalizationData locale)
        {
            return randomNumber.Next(2) == 0 ? GeneratePersonName(randomNumber, locale) : GenerateBandName(randomNumber, locale);
        }

        private string GeneratePersonName(Random randomNumber, LocalizationData locale)
        {
            var prefix = randomNumber.Next(4) == 0 ? Pick(randomNumber, locale.ArtistPrefixes) + " " : string.Empty;

            return $"{prefix}{Pick(randomNumber, locale.ArtistFirstNames)} {Pick(randomNumber, locale.ArtistLastNames)}";
        }

        private string GenerateBandName(Random randomNumber, LocalizationData locale)
        {
            var part1 = Pick(randomNumber, locale.BandNameParts);
            var part2 = Pick(randomNumber, locale.BandNameParts);
            var lastName = Pick(randomNumber, locale.ArtistLastNames);

            var patterns = new List<string>
            {
                $"{part1} {part2}",
                $"The {part1}s",
                $"{lastName} & The Band",
                $"Dr.{lastName}",
                $"{part1} Project"
            };

            return Pick(randomNumber, patterns);
        }

        private int GenerateLikes(Random randomNumber, double avgLikes)
        {
            double likes = Math.Clamp(avgLikes, 0, 10);
            return ExecuteTimes(likes, randomNumber);
        }

        private string GenerateLyrics(Random rnd, LocalizationData locale)
        {
            int linesCount = ExecuteTimes(8.4, rnd);

            var lines = new List<string>();
            for (int i = 0; i < linesCount; i++)
            {
                var phrase = Pick(rnd, locale.ReviewPhrases);
                lines.Add(phrase);
            }
            return string.Join("\n", lines);
        }

        private T Pick<T>(Random randomNumber, List<T> list)
            => list[randomNumber.Next(list.Count)];

        private int ExecuteTimes(double n, Random rng)
        {
            int baseValue = (int)Math.Floor(n);
            double fraction = n % 1;
            return rng.NextDouble() < fraction ? baseValue + 1 : baseValue;
        }
    }
}
