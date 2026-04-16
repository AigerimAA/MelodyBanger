using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Extensions.Caching.Memory;

namespace MelodyBanger.Services
{
    public class MusicGeneratorService
    {
        private readonly IMemoryCache _cache;

        private static readonly Dictionary<string, int[][]> ChordProgressions = new()
        {
            ["Pop"] = new[] { new[] { 0, 4, 9, 5 }, new[] { 0, 5, 9, 7 }, new[] { 0, 4, 7, 5 }, new[] { 0, 3, 9, 5 } },
            ["Rock"] = new[] { new[] { 0, 7, 5, 4 }, new[] { 0, 3, 7, 5 }, new[] { 0, 5, 7, 4 }, new[] { 0, 7, 3, 5 } },
            ["Jazz"] = new[] { new[] { 0, 5, 7, 2 }, new[] { 2, 7, 0, 5 }, new[] { 0, 5, 9, 4 }, new[] { 0, 4, 7, 2 } },
            ["Electronic"] = new[] { new[] { 0, 3, 7, 10 }, new[] { 0, 5, 7, 3 }, new[] { 0, 7, 10, 5 }, new[] { 0, 8, 3, 7 } },
            ["House"] = new[] { new[] { 0, 5, 9, 7 }, new[] { 0, 3, 5, 10 }, new[] { 0, 7, 5, 9 }, new[] { 0, 5, 8, 3 } },
            ["Ballad"] = new[] { new[] { 0, 4, 9, 5 }, new[] { 0, 9, 5, 7 }, new[] { 0, 4, 7, 9 }, new[] { 0, 4, 9, 2 } },
            ["Blues"] = new[] { new[] { 0, 0, 0, 5, 5, 0, 7, 5, 0, 7 }, new[] { 0, 5, 0, 7, 5, 0, 7, 5, 0, 7 } },
            ["Folk"] = new[] { new[] { 0, 4, 7, 5 }, new[] { 0, 5, 7, 4 }, new[] { 0, 4, 9, 7 }, new[] { 0, 3, 7, 5 } },
            ["default"] = new[] { new[] { 0, 4, 7, 5 }, new[] { 0, 5, 7, 4 }, new[] { 0, 4, 9, 7 } }
        };

        private static readonly int[] RootNotes = { 48, 50, 52, 53, 55, 57, 59, 60, 62, 64, 65, 67, 69, 71, 72 };

        private static readonly int[] PianoPrograms = { 0, 1, 2, 3, 4, 5, 6, 7 };

        public MusicGeneratorService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public byte[] GenerateMusic(long seed, int songIndex, string genre)
        {
            var cacheKey = $"music_{seed}_{songIndex}";
            return _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return CreateMidi(seed, songIndex, genre);
            })!;
        }

        private byte[] CreateMidi(long seed, int songIndex, string genre)
        {
            var rnd = new Random(HashCode.Combine(seed, songIndex));
            var tempo = PickTempo(rnd, genre);
            var rootNote = RootNotes[rnd.Next(RootNotes.Length)];
            var pianoProgram = PianoPrograms[rnd.Next(PianoPrograms.Length)];
            var hasIntro = rnd.Next(2) == 0;
            var hasOutro = rnd.Next(2) == 0;

            var midiFile = new MidiFile();

            midiFile.Chunks.Add(CreatePianoTrack(rnd, genre, tempo, rootNote, pianoProgram, hasIntro, hasOutro));
            midiFile.Chunks.Add(CreateMelodyTrack(rnd, genre, tempo, rootNote, hasIntro, hasOutro));
            midiFile.Chunks.Add(CreateBassTrack(rnd, genre, tempo, rootNote, hasIntro, hasOutro));
            midiFile.Chunks.Add(CreateDrumTrack(rnd, genre, tempo, hasIntro, hasOutro));

            if (rnd.Next(3) == 0)
            {
                midiFile.Chunks.Add(CreatePadTrack(rnd, genre, tempo, rootNote));
            }

            using var ms = new MemoryStream();
            midiFile.Write(ms);
            return ms.ToArray();
        }

        private TrackChunk CreatePianoTrack(Random rnd, string genre, int tempo, int rootNote, int pianoProgram, bool hasIntro, bool hasOutro)
        {
            var chunk = new TrackChunk();
            using var manager = chunk.ManageNotes();
            var progression = PickProgression(rnd, genre);

            long time = 0;
            int introBars = hasIntro ? 2 : 0;
            int outroBars = hasOutro ? 2 : 0;
            int mainBars = 16;

            if (hasIntro)
            {
                for (int bar = 0; bar < introBars; bar++)
                {
                    var chordRoot = progression[bar % progression.Length];
                    var pitch = (SevenBitNumber)Math.Clamp(rootNote + chordRoot + 7, 0, 127);
                    for (int beat = 0; beat < 4; beat++)
                    {
                        manager.Objects.Add(new Note(pitch, 200, time) { Velocity = (SevenBitNumber)50 });
                        time += 240;
                    }
                }
            }

            for (int bar = 0; bar < mainBars; bar++)
            {
                var chordRoot = progression[bar % progression.Length];
                var volume = bar < 4 ? (byte)65 : (byte)80;

                for (int beat = 0; beat < 4; beat++)
                {
                    var pitch = (SevenBitNumber)Math.Clamp(rootNote + chordRoot + 7, 0, 127);
                    manager.Objects.Add(new Note(pitch, 220, time) { Velocity = (SevenBitNumber)volume });
                    time += 240;
                }

                if (bar == 8)
                {
                    var bridgePitch = (SevenBitNumber)Math.Clamp(rootNote + chordRoot + 12, 0, 127);
                    manager.Objects.Add(new Note(bridgePitch, 480, time - 240) { Velocity = (SevenBitNumber)90 });
                }
            }

            if (hasOutro)
            {
                for (int bar = 0; bar < outroBars; bar++)
                {
                    var chordRoot = progression[(mainBars - 1) % progression.Length];
                    var pitch = (SevenBitNumber)Math.Clamp(rootNote + chordRoot + 7, 0, 127);
                    var volume = (byte)(60 - bar * 10);
                    manager.Objects.Add(new Note(pitch, 920, time) { Velocity = (SevenBitNumber)volume });
                    time += 960;
                }
            }

            return chunk;
        }

        private TrackChunk CreateMelodyTrack(Random rnd, string genre, int tempo, int rootNote, bool hasIntro, bool hasOutro)
        {
            var chunk = new TrackChunk();
            using var manager = chunk.ManageNotes();
            var progression = PickProgression(rnd, genre);

            var melodyPatterns = new[]
            {
                new[] { 0, 2, 4, 5, 7, 9, 7, 5, 4, 2, 0, -2, -4, -5, -7, -9 },
                new[] { 0, 3, 5, 7, 5, 3, 0, -3, -5, -7, -5, -3, 0, -2, -4, -5 },
                new[] { 0, 4, 7, 9, 7, 4, 0, -4, -7, -9, -7, -4, 0, 2, 4, 5 },
                new[] { 0, 1, 3, 5, 7, 5, 3, 1, 0, -1, -3, -5, -7, -5, -3, -1 },
                new[] { 0, 5, 7, 9, 10, 9, 7, 5, 0, -5, -7, -9, -10, -9, -7, -5 }
            };

            var pattern = melodyPatterns[rnd.Next(melodyPatterns.Length)];
            long time = hasIntro ? 960 : 480;
            int step = 0;
            int melodyLength = 32;

            for (int bar = 0; bar < 16; bar++)
            {
                var chordRoot = progression[bar % progression.Length];
                var volume = bar < 4 ? (byte)70 : (byte)95;

                if (bar == 8)
                {
                    volume = 105;
                }

                for (int i = 0; i < 2; i++)
                {
                    var interval = pattern[step % pattern.Length];
                    var pitch = (SevenBitNumber)Math.Clamp(rootNote + 12 + chordRoot + interval, 0, 127);
                    var duration = 440;

                    if (step % 8 == 0 && step > 0)
                    {
                        time += 40;
                    }

                    manager.Objects.Add(new Note(pitch, duration, time)
                    {
                        Velocity = (SevenBitNumber)volume
                    });

                    time += 480;
                    step++;

                    if (step >= melodyLength) break;
                }
                if (step >= melodyLength) break;
            }

            return chunk;
        }

        private TrackChunk CreateBassTrack(Random rnd, string genre, int tempo, int rootNote, bool hasIntro, bool hasOutro)
        {
            var chunk = new TrackChunk();
            using var manager = chunk.ManageNotes();
            var progression = PickProgression(rnd, genre);

            long time = 0;
            int introBars = hasIntro ? 2 : 0;
            int outroBars = hasOutro ? 2 : 0;
            int mainBars = 16;

            if (hasIntro)
            {
                for (int bar = 0; bar < introBars; bar++)
                {
                    var chordRoot = progression[bar % progression.Length];
                    var pitch = (SevenBitNumber)Math.Clamp(rootNote - 12 + chordRoot, 0, 127);
                    manager.Objects.Add(new Note(pitch, 920, time) { Velocity = (SevenBitNumber)70 });
                    time += 960;
                }
            }

            for (int bar = 0; bar < mainBars; bar++)
            {
                var chordRoot = progression[bar % progression.Length];
                var pitch = (SevenBitNumber)Math.Clamp(rootNote - 12 + chordRoot, 0, 127);
                var volume = bar < 4 ? (byte)80 : (byte)95;

                if (rnd.Next(3) == 0)
                {
                    manager.Objects.Add(new Note(pitch, 200, time) { Velocity = (SevenBitNumber)volume });
                    manager.Objects.Add(new Note(pitch, 200, time + 480) { Velocity = (SevenBitNumber)volume });
                    time += 960;
                }
                else
                {
                    manager.Objects.Add(new Note(pitch, 920, time) { Velocity = (SevenBitNumber)volume });
                    time += 960;
                }
            }

            if (hasOutro)
            {
                for (int bar = 0; bar < outroBars; bar++)
                {
                    var chordRoot = progression[(mainBars - 1) % progression.Length];
                    var pitch = (SevenBitNumber)Math.Clamp(rootNote - 12 + chordRoot, 0, 127);
                    var volume = (byte)(80 - bar * 15);
                    manager.Objects.Add(new Note(pitch, 920, time) { Velocity = (SevenBitNumber)volume });
                    time += 960;
                }
            }

            return chunk;
        }

        private TrackChunk CreateDrumTrack(Random rnd, string genre, int tempo, bool hasIntro, bool hasOutro)
        {
            var chunk = new TrackChunk();
            using var manager = chunk.ManageNotes();

            long time = 0;
            int introBars = hasIntro ? 2 : 0;
            int outroBars = hasOutro ? 2 : 0;
            int mainBars = 32;

            for (int i = 0; i < introBars * 4; i++)
            {
                if (i % 4 == 0)
                    manager.Objects.Add(new Note((SevenBitNumber)36, 200, time) { Velocity = (SevenBitNumber)90 });
                time += 240;
            }

            for (int i = 0; i < mainBars * 4; i++)
            {
                var volume = i < 16 ? 100 : 115;

                if (i % 4 == 0)
                    manager.Objects.Add(new Note((SevenBitNumber)36, 200, time) { Velocity = (SevenBitNumber)volume });

                if (i % 4 == 2)
                    manager.Objects.Add(new Note((SevenBitNumber)38, 200, time) { Velocity = (SevenBitNumber)(volume - 10) });

                if (i % 2 == 0)
                    manager.Objects.Add(new Note((SevenBitNumber)42, 100, time) { Velocity = (SevenBitNumber)85 });

                if (i % 8 == 6)
                    manager.Objects.Add(new Note((SevenBitNumber)49, 150, time) { Velocity = (SevenBitNumber)95 });

                time += 240;
            }

            for (int i = 0; i < outroBars * 4; i++)
            {
                if (i % 4 == 0)
                    manager.Objects.Add(new Note((SevenBitNumber)36, 200, time) { Velocity = (SevenBitNumber)80 });
                time += 240;
            }

            return chunk;
        }

        private TrackChunk CreatePadTrack(Random rnd, string genre, int tempo, int rootNote)
        {
            var chunk = new TrackChunk();
            using var manager = chunk.ManageNotes();
            var progression = PickProgression(rnd, genre);

            long time = 480;

            for (int bar = 0; bar < 16; bar++)
            {
                var chordRoot = progression[bar % progression.Length];
                var pitch = (SevenBitNumber)Math.Clamp(rootNote + chordRoot + 16, 0, 127);
                manager.Objects.Add(new Note(pitch, 940, time)
                {
                    Velocity = (SevenBitNumber)55
                });
                time += 960;
            }

            return chunk;
        }

        private int[] PickProgression(Random rnd, string genre)
        {
            var key = ChordProgressions.ContainsKey(genre) ? genre : "default";
            var options = ChordProgressions[key];
            return options[rnd.Next(options.Length)];
        }

        private int PickTempo(Random rnd, string genre) => genre switch
        {
            "Pop" => rnd.Next(110, 135),
            "Rock" => rnd.Next(120, 150),
            "Jazz" => rnd.Next(100, 130),
            "Blues" => rnd.Next(75, 105),
            "Electronic" => rnd.Next(125, 140),
            "House" => rnd.Next(120, 135),
            "Ballad" => rnd.Next(65, 85),
            "Folk" => rnd.Next(85, 110),
            _ => rnd.Next(100, 125)
        };

        public double GetMusicDuration(long seed, int songIndex, string genre)
        {
            var rnd = new Random(HashCode.Combine(seed, songIndex));
            var tempo = PickTempo(rnd, genre);
            return 32.0 * 60.0 / tempo;
        }
    }
}