using MelodyBanger.Models;
using Bogus;

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
                var contentRandom = new Random(songSeed);     
                var likesRandom = new Random(songSeed);

                songs.Add(new Song
                {
                    Index = globalIndex,
                    Title = GenerateTitle(contentRandom, locale),      
                    Artist = GenerateArtist(contentRandom, locale, p.Lang),   
                    Album = GenerateAlbum(contentRandom, locale),    
                    Genre = Pick(contentRandom, locale.Genres),       
                    Likes = GenerateLikes(likesRandom, p.Likes),  
                    Review = Pick(contentRandom, locale.ReviewPhrases),
                    Lyrics = GenerateLyrics(contentRandom, locale)
                });
            }

            return new SongPage
            {
                Items = songs,
                CurrentPage = p.Page,
                TotalPages = int.MaxValue,
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

        private string GenerateArtist(Random randomNumber, LocalizationData locale, string lang)
        {
            if (lang == "kz")
            {
                return randomNumber.Next(2) == 0 
                    ? GeneratePersonName(randomNumber, locale) 
                    : GenerateBandName(randomNumber, locale);
            }
            
            var faker = new Faker(lang);
            
            if (randomNumber.Next(2) == 0)
            {
                return faker.Name.FullName();
            }
            else
            {
                var bandNames = new[] { "The ", "The ", "", "", "The " };
                var suffix = new[] { "Band", "Project", "Experience", "Collective", "Sound" };
                return bandNames[randomNumber.Next(bandNames.Length)] + faker.Random.WordsArray(1, 2)[0] + " " + suffix[randomNumber.Next(suffix.Length)];
            }
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
                var line = Pick(rnd, locale.LyricsLines);
                lines.Add(line);
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
