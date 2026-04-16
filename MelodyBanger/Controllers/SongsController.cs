using MelodyBanger.Models;
using MelodyBanger.Services;
using Microsoft.AspNetCore.Mvc;

namespace MelodyBanger.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private readonly SongGeneratorService _generator;
        private readonly CoverGeneratorService _coverGenerator;
        private readonly MusicGeneratorService _musicGenerator;
        private readonly LocalizationService _localization;

        public SongsController(SongGeneratorService generator, CoverGeneratorService coverGenerator, MusicGeneratorService musicGenerator, LocalizationService localization)
        {
            _generator = generator;
            _coverGenerator = coverGenerator;
            _musicGenerator = musicGenerator;
            _localization = localization;
        }

        [HttpGet]
        public async Task<IActionResult> GetPage([FromQuery] GenerationParams p)
        {
            var page = await _generator.GeneratePage(p);
            return Ok(page);
        }

        [HttpGet("cover")]
        public IActionResult GetCover(
            [FromQuery] long seed,
            [FromQuery] int index,
            [FromQuery] string title,
            [FromQuery] string artist,
            [FromQuery] string genre = "")
        {
            var imageBytes = _coverGenerator.GenerateCover(seed, index, title, artist, genre);
            return File(imageBytes, "image/jpeg");
        }


        [HttpGet("music")]
        public IActionResult GetMusic(
            [FromQuery] long seed,
            [FromQuery] int index,
            [FromQuery] string genre)
        {
            var midiData = _musicGenerator.GenerateMusic(seed, index, genre);
            return File(midiData, "audio/midi", $"song_{seed}_{index}.mid");
        }
    }
}
