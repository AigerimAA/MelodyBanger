using Microsoft.Extensions.Caching.Memory;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing.Shapes;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MelodyBanger.Services
{
    public class CoverGeneratorService
    {
        private readonly IMemoryCache _cache;
        private const int CoverSize = 400;
        private int _lastStyle = -1;

        public CoverGeneratorService(IMemoryCache cache)
        {
            _cache = cache;
        }

        private static readonly Rgba32[][] Palettes =
        [
            new Rgba32[] { new(30, 20, 80), new(80, 40, 160), new(255, 100, 220), new(20, 180, 200) },
            new Rgba32[] { new(20, 60, 140), new(40, 140, 200), new(0, 240, 255), new(20, 20, 40) },
            new Rgba32[] { new(120, 20, 40), new(200, 50, 60), new(255, 120, 80), new(40, 10, 20) },
            new Rgba32[] { new(20, 80, 60), new(40, 160, 100), new(0, 255, 160), new(10, 30, 20) },
            new Rgba32[] { new(100, 80, 20), new(200, 140, 40), new(255, 220, 80), new(40, 30, 10) },
            new Rgba32[] { new(160, 60, 200), new(220, 100, 255), new(100, 20, 160), new(40, 10, 60) },
            new Rgba32[] { new(240, 240, 240), new(200, 200, 200), new(50, 50, 50), new(0, 0, 0) },
            new Rgba32[] { new(255, 180, 100), new(255, 100, 150), new(100, 150, 255), new(20, 20, 40) },
            new Rgba32[] { new(0, 200, 255), new(255, 0, 150), new(255, 255, 0), new(0, 0, 0) },
            new Rgba32[] { new(60, 200, 60), new(200, 200, 60), new(60, 60, 200), new(20, 20, 20) },
            new Rgba32[] { new(255, 100, 50), new(50, 150, 255), new(200, 50, 200), new(10, 10, 30) },
            new Rgba32[] { new(20, 30, 50), new(40, 80, 120), new(100, 160, 200), new(200, 220, 240) }
        ];

        private static readonly Dictionary<string, string[]> GenreWords = new()
        {
            ["Pop"] = new[] { "LOVE", "DREAM", "STAR", "SHINE", "HEART", "SOUL" },
            ["Rock"] = new[] { "FIRE", "WILD", "LOUD", "STORM", "RAGE", "CHAOS" },
            ["Jazz"] = new[] { "SOUL", "COOL", "BLUE", "VIBE", "LATIN", "GROOVE" },
            ["Blues"] = new[] { "BLUE", "SOUL", "DEEP", "SLOW", "CROSS", "DELTA" },
            ["Electronic"] = new[] { "NEON", "WAVE", "BEAT", "PULSE", "SYNC", "LASER" },
            ["House"] = new[] { "MOVE", "FEEL", "VIBE", "RISE", "DANCE", "GROOVE" },
            ["Ballad"] = new[] { "LOVE", "HOPE", "SOFT", "PURE", "PEACE", "CALM" },
            ["Folk"] = new[] { "HOME", "ROAD", "FREE", "WILD", "MOUNTAIN", "RIVER" },
            ["Country"] = new[] { "DIRT", "TRUCK", "NIGHT", "MOON", "CREEK", "DUST" },
            ["R&B"] = new[] { "SLOW", "SMOOTH", "HONEY", "GOLD", "NIGHT", "URBAN" },
            ["Dream Pop"] = new[] { "CLOUD", "GHOST", "FLOAT", "DRIFT", "HAZE", "GLOW" },
            ["default"] = new[] { "MUSIC", "BEAT", "SONG", "PLAY", "SOUND", "RHYTHM" }
        };

        public byte[] GenerateCover(long seed, int songIndex, string title, string artist, string genre = "")
        {
            var cacheKey = $"cover_{seed}_{songIndex}";
            return _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return CreateCover(seed, songIndex, title, artist, genre);
            })!;
        }

        private byte[] CreateCover(long seed, int songIndex, string title, string artist, string genre)
        {
            var rng = new Random(HashCode.Combine(seed, songIndex));
            using var image = new Image<Rgba32>(CoverSize, CoverSize);

            var style = rng.Next(29);
            while (style == _lastStyle)
            {
                style = rng.Next(29);
            }
            _lastStyle = style;

            if (style < 9)
            {
                var palette = Palettes[rng.Next(Palettes.Length)];
                var bgColor = palette[rng.Next(palette.Length)];
                image.Mutate(ctx =>
                {
                    ctx.Fill(bgColor);
                    DrawArtisticPattern(ctx, rng, palette, CoverSize, genre);
                    DrawTextWithBackdrop(ctx, title, artist, CoverSize);
                });
            }
            else
            {
                DrawRealisticCover(image, rng, title, artist, genre);
            }

            using var ms = new MemoryStream();
            image.SaveAsJpeg(ms);
            return ms.ToArray();
        }

        private void DrawRealisticCover(Image<Rgba32> image, Random rng, string title, string artist, string genre)
        {
            var style = rng.Next(18);

            switch (style)
            {
                case 0: DrawSunsetPortrait(image, rng, title, artist); break;
                case 1: DrawNatureMountain(image, rng, title, artist); break;
                case 2: DrawOceanWave(image, rng, title, artist); break;
                case 3: DrawNeonAbstract(image, rng, title, artist); break;
                case 4: DrawSilhouette(image, rng, title, artist); break;
                case 5: DrawWatercolorSplash(image, rng, title, artist); break;
                case 6: DrawCityscape(image, rng, title, artist); break;
                case 7: DrawAbstractGradient(image, rng, title, artist); break;
                case 8: DrawStarryNight(image, rng, title, artist); break;
                case 9: DrawForestPath(image, rng, title, artist); break;
                case 10: DrawDesertDunes(image, rng, title, artist); break;
                case 11: DrawAuroraBorealis(image, rng, title, artist); break;
                case 12: DrawCherryBlossom(image, rng, title, artist); break;
                case 13: DrawVintageRecord(image, rng, title, artist); break;
                case 14: DrawGeometricPattern(image, rng, title, artist); break;
                case 15: DrawFireFlames(image, rng, title, artist); break;
                case 16: DrawUnderwater(image, rng, title, artist); break;
                case 17: DrawGalaxy(image, rng, title, artist); break;
                case 18: DrawSeaWaves(image, rng, title, artist); break;
                case 19: DrawImpressionistCity(image, rng, title, artist); break;
            }
        }

        private void DrawSunsetPortrait(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var r = (byte)(255 * (1 - t) + 100 * t);
                    var g = (byte)(100 * (1 - t) + 50 * t);
                    var b = (byte)(50 * (1 - t) + 20 * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }

                var sunY = CoverSize * 0.3f;
                var sunR = CoverSize * 0.12f;
                ctx.Fill(Color.FromRgb(255, 200, 100), new EllipsePolygon(CoverSize / 2, sunY, sunR, sunR));

                var bodyPoints = new PointF[]
                {
                    new(CoverSize * 0.45f, CoverSize * 0.7f),
                    new(CoverSize * 0.55f, CoverSize * 0.7f),
                    new(CoverSize * 0.5f, CoverSize * 0.85f),
                };
                ctx.Fill(Color.Black, new Polygon(bodyPoints));

                var head = new EllipsePolygon(CoverSize * 0.5f, CoverSize * 0.62f, CoverSize * 0.05f, CoverSize * 0.07f);
                ctx.Fill(Color.Black, head);

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawNatureMountain(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var r = (byte)(135 * (1 - t) + 80 * t);
                    var g = (byte)(206 * (1 - t) + 100 * t);
                    var b = (byte)(235 * (1 - t) + 60 * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }

                var mountainPoints = new PointF[]
                {
                    new(0, CoverSize * 0.7f),
                    new(CoverSize * 0.2f, CoverSize * 0.35f),
                    new(CoverSize * 0.4f, CoverSize * 0.55f),
                    new(CoverSize * 0.6f, CoverSize * 0.25f),
                    new(CoverSize * 0.8f, CoverSize * 0.45f),
                    new(CoverSize, CoverSize * 0.55f),
                    new(CoverSize, CoverSize),
                    new(0, CoverSize)
                };
                ctx.Fill(Color.FromRgb(70, 90, 70), new Polygon(mountainPoints));

                var snowPoints = new PointF[]
                {
                    new(CoverSize * 0.57f, CoverSize * 0.27f),
                    new(CoverSize * 0.63f, CoverSize * 0.32f),
                    new(CoverSize * 0.53f, CoverSize * 0.33f),
                };
                ctx.Fill(Color.White, new Polygon(snowPoints));

                var secondPeak = new PointF[]
                {
                    new(CoverSize * 0.17f, CoverSize * 0.38f),
                    new(CoverSize * 0.23f, CoverSize * 0.43f),
                    new(CoverSize * 0.12f, CoverSize * 0.44f),
                };
                ctx.Fill(Color.White, new Polygon(secondPeak));

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawOceanWave(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var r = (byte)(20 + 80 * t);
                    var g = (byte)(60 + 100 * t);
                    var b = (byte)(180 + 50 * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }

                for (int i = 0; i < 4; i++)
                {
                    var waveY = CoverSize * 0.4f + i * 50;
                    var amplitude = 15 - i * 3;
                    var points = new List<PointF>();
                    for (int x = 0; x <= CoverSize; x += 20)
                    {
                        points.Add(new PointF(x, waveY + MathF.Sin(x * 0.02f + i) * amplitude));
                    }
                    points.Add(new PointF(CoverSize, CoverSize));
                    points.Add(new PointF(0, CoverSize));
                    var alpha = (byte)(180 - i * 30);
                    ctx.Fill(Color.FromRgba(255, 255, 255, alpha), new Polygon(points.ToArray()));
                }

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawNeonAbstract(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.Black);

                var neonColors = new[] { Color.HotPink, Color.Cyan, Color.LimeGreen, Color.Yellow, Color.Magenta };

                for (int i = 0; i < 25; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var size = rng.Next(40, 180);
                    var color = neonColors[rng.Next(neonColors.Length)];
                    var thickness = rng.Next(3, 10);

                    ctx.Draw(color, thickness, new EllipsePolygon(x, y, size, size));
                    ctx.Draw(Color.White, 1, new EllipsePolygon(x, y, size + 3, size + 3));
                }

                for (int i = 0; i < 30; i++)
                {
                    var x1 = rng.Next(CoverSize);
                    var y1 = rng.Next(CoverSize);
                    var x2 = x1 + rng.Next(-100, 100);
                    var y2 = y1 + rng.Next(-100, 100);
                    var color = neonColors[rng.Next(neonColors.Length)];
                    ctx.DrawLine(color, rng.Next(2, 6), new PointF(x1, y1), new PointF(x2, y2));
                }

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawSilhouette(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                var palette = Palettes[rng.Next(Palettes.Length)];
                var gradStart = palette[0];
                var gradEnd = palette[1];

                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var r = (byte)(gradStart.R * (1 - t) + gradEnd.R * t);
                    var g = (byte)(gradStart.G * (1 - t) + gradEnd.G * t);
                    var b = (byte)(gradStart.B * (1 - t) + gradEnd.B * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }

                var cityPoints = new List<PointF>();
                cityPoints.Add(new PointF(0, CoverSize));
                for (int x = 0; x <= CoverSize; x += 15)
                {
                    var height = CoverSize * (0.2f + rng.Next(10, 40) / 100f);
                    cityPoints.Add(new PointF(x, height));
                }
                cityPoints.Add(new PointF(CoverSize, CoverSize));
                ctx.Fill(Color.Black, new Polygon(cityPoints.ToArray()));

                for (int i = 0; i < 15; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize / 2, CoverSize);
                    var size = rng.Next(2, 6);
                    ctx.Fill(Color.FromRgb(255, 200, 100), new EllipsePolygon(x, y, size, size));
                }

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawWatercolorSplash(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.FromRgb(250, 245, 240));

                for (int i = 0; i < 30; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var radius = rng.Next(20, 80);
                    var alpha = (byte)rng.Next(30, 100);
                    var r = (byte)rng.Next(100, 220);
                    var g = (byte)rng.Next(100, 200);
                    var b = (byte)rng.Next(150, 255);
                    ctx.Fill(Color.FromRgba(r, g, b, alpha), new EllipsePolygon(x, y, radius, radius));
                }

                for (int i = 0; i < 50; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var radius = rng.Next(5, 20);
                    var alpha = (byte)rng.Next(20, 60);
                    ctx.Fill(Color.FromRgba(100, 50, 150, alpha), new EllipsePolygon(x, y, radius, radius));
                }

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawCityscape(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var r = (byte)(20 + 30 * t);
                    var g = (byte)(10 + 40 * t);
                    var b = (byte)(40 + 60 * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }

                var buildingCount = rng.Next(12, 20);
                var buildingWidth = CoverSize / buildingCount;

                for (int i = 0; i < buildingCount; i++)
                {
                    var height = CoverSize * (0.2f + rng.Next(20, 60) / 100f);
                    var x = i * buildingWidth;
                    var brightness = rng.Next(40, 100);
                    var color = Color.FromRgb((byte)brightness, (byte)brightness, (byte)(brightness + 20));
                    ctx.Fill(color, new RectangleF(x, CoverSize - height, buildingWidth - 2, height));

                    for (int w = 0; w < rng.Next(3, 8); w++)
                    {
                        var windowX = x + rng.Next(5, (int)buildingWidth - 10);
                        var windowY = CoverSize - height + rng.Next(10, (int)height - 10);
                        ctx.Fill(Color.FromRgb(255, 220, 100), new RectangleF(windowX, windowY, 5, 8));
                    }
                }

                for (int i = 0; i < 30; i++)
                {
                    var starX = rng.Next(CoverSize);
                    var starY = rng.Next(CoverSize / 2);
                    ctx.Fill(Color.White, new EllipsePolygon(starX, starY, 2, 2));
                }

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawAbstractGradient(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                var palette = Palettes[rng.Next(Palettes.Length)];
                var c1 = palette[0];
                var c2 = palette[1];
                var c3 = palette[2];

                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var r = (byte)(c1.R * (1 - t) + c2.R * t);
                    var g = (byte)(c1.G * (1 - t) + c2.G * t);
                    var b = (byte)(c1.B * (1 - t) + c2.B * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }

                for (int i = 0; i < 5; i++)
                {
                    var yPos = CoverSize * (0.2f + i * 0.15f);
                    var thickness = rng.Next(10, 40);
                    var alpha = (byte)rng.Next(80, 160);
                    ctx.Fill(Color.FromRgba((byte)c3.R, (byte)c3.G, (byte)c3.B, alpha), new RectangleF(0, yPos, CoverSize, thickness));
                }

                for (int i = 0; i < 50; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var radius = rng.Next(3, 15);
                    var alpha = (byte)rng.Next(100, 200);
                    ctx.Fill(Color.FromRgba(255, 255, 255, alpha), new EllipsePolygon(x, y, radius, radius));
                }

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawStarryNight(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.FromRgb(10, 20, 50));
                for (int i = 0; i < 150; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var size = rng.Next(1, 4);
                    ctx.Fill(Color.White, new EllipsePolygon(x, y, size, size));
                }
                var moonX = CoverSize - 60;
                var moonY = 50;
                ctx.Fill(Color.FromRgb(255, 240, 200), new EllipsePolygon(moonX, moonY, 30, 30));
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawForestPath(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var g = (byte)(80 + 80 * t);
                    var r = (byte)(40 + 40 * t);
                    var b = (byte)(20 + 20 * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }
                var pathPoints = new PointF[]
                {
                    new(CoverSize * 0.3f, CoverSize),
                    new(CoverSize * 0.4f, CoverSize * 0.7f),
                    new(CoverSize * 0.5f, CoverSize * 0.5f),
                    new(CoverSize * 0.6f, CoverSize * 0.7f),
                    new(CoverSize * 0.7f, CoverSize)
                };
                ctx.Fill(Color.FromRgb(120, 80, 40), new Polygon(pathPoints));
                for (int i = 0; i < 20; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var size = rng.Next(15, 40);
                    ctx.Fill(Color.FromRgb(50, 80, 40), new EllipsePolygon(x, y, size, size));
                }
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawDesertDunes(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.FromRgb(210, 180, 140));
                for (int i = 0; i < 5; i++)
                {
                    var yOffset = CoverSize * 0.5f + i * 60;
                    var points = new List<PointF>();
                    for (int x = 0; x <= CoverSize; x += 30)
                    {
                        points.Add(new PointF(x, yOffset + MathF.Sin(x * 0.01f + i) * 40));
                    }
                    points.Add(new PointF(CoverSize, CoverSize));
                    points.Add(new PointF(0, CoverSize));
                    ctx.Fill(Color.FromRgb((byte)(180 + i * 10), (byte)(150 - i * 10), (byte)100), new Polygon(points.ToArray()));
                }
                var sunX = CoverSize - 60;
                var sunY = 60;
                ctx.Fill(Color.FromRgb(255, 180, 80), new EllipsePolygon(sunX, sunY, 40, 40));
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawAuroraBorealis(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.FromRgb(10, 20, 40));
                for (int i = 0; i < 8; i++)
                {
                    var yStart = rng.Next(50, 200);
                    var alpha = (byte)rng.Next(80, 150);
                    var r = (byte)rng.Next(0, 100);
                    var g = (byte)rng.Next(150, 255);
                    var b = (byte)rng.Next(100, 200);
                    var points = new List<PointF>();
                    for (int x = 0; x <= CoverSize; x += 20)
                    {
                        points.Add(new PointF(x, yStart + MathF.Sin(x * 0.03f + i) * 30));
                    }
                    points.Add(new PointF(CoverSize, CoverSize));
                    points.Add(new PointF(0, CoverSize));
                    ctx.Fill(Color.FromRgba(r, g, b, alpha), new Polygon(points.ToArray()));
                }
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawCherryBlossom(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.FromRgb(255, 220, 240));
                for (int i = 0; i < 50; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var size = rng.Next(5, 12);
                    ctx.Fill(Color.FromRgb(255, 150, 180), new EllipsePolygon(x, y, size, size));
                    ctx.Fill(Color.FromRgb(255, 200, 220), new EllipsePolygon(x - 3, y - 2, size - 2, size - 2));
                }
                var branchPoints = new PointF[]
                {
                    new(0, CoverSize * 0.3f),
                    new(CoverSize * 0.4f, CoverSize * 0.35f),
                    new(CoverSize, CoverSize * 0.25f)
                };
                ctx.DrawLine(Color.FromRgb(80, 50, 30), 8, branchPoints);
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawVintageRecord(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.Black);
                var centerX = CoverSize / 2;
                var centerY = CoverSize / 2;
                ctx.Draw(Color.FromRgb(80, 80, 80), 4, new EllipsePolygon(centerX, centerY, 150, 150));
                ctx.Draw(Color.FromRgb(150, 150, 150), 2, new EllipsePolygon(centerX, centerY, 140, 140));
                ctx.Fill(Color.FromRgb(200, 50, 50), new EllipsePolygon(centerX, centerY, 20, 20));
                for (int i = 0; i < 8; i++)
                {
                    var angle = i * Math.PI * 2 / 8;
                    var x = centerX + MathF.Cos((float)angle) * 100;
                    var y = centerY + MathF.Sin((float)angle) * 100;
                    ctx.Fill(Color.White, new EllipsePolygon(x, y, 3, 3));
                }
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawGeometricPattern(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.FromRgb(20, 20, 40));
                for (int i = 0; i < 20; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var size = rng.Next(30, 100);
                    var r = (byte)rng.Next(100, 255);
                    var g = (byte)rng.Next(50, 200);
                    var b = (byte)rng.Next(50, 200);
                    ctx.Draw(Color.FromRgb(r, g, b), 3, new RectangularPolygon(x, y, size, size));
                }
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawFireFlames(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.FromRgb(30, 10, 0));
                for (int i = 0; i < 20; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize / 2, CoverSize);
                    var width = rng.Next(20, 60);
                    var height = rng.Next(40, 120);
                    var color = rng.Next(3) switch
                    {
                        0 => Color.FromRgb(255, 80, 0),
                        1 => Color.FromRgb(255, 150, 0),
                        _ => Color.FromRgb(255, 220, 100)
                    };
                    ctx.Fill(color, new EllipsePolygon(x, y, width, height));
                }
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawUnderwater(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var b = (byte)(200 + 55 * t);
                    var g = (byte)(100 + 80 * t);
                    var r = (byte)(50 + 30 * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }
                for (int i = 0; i < 15; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var size = rng.Next(10, 30);
                    ctx.Fill(Color.FromRgb(80, 200, 150), new EllipsePolygon(x, y, size, size));
                }
                var bubblePoints = new PointF[]
                {
                    new(50, 300), new(55, 280), new(48, 260), new(52, 240)
                };
                ctx.DrawLine(Color.White, 2, bubblePoints);
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawGalaxy(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.FromRgb(15, 10, 30));
                for (int i = 0; i < 200; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var size = rng.Next(1, 3);
                    ctx.Fill(Color.White, new EllipsePolygon(x, y, size, size));
                }
                var spiral = new List<PointF>();
                for (int t = 0; t < 100; t++)
                {
                    var angle = t * 0.3f;
                    var radius = t * 3f;
                    var x = CoverSize / 2 + MathF.Cos(angle) * radius;
                    var y = CoverSize / 2 + MathF.Sin(angle) * radius;
                    if (x > 0 && x < CoverSize && y > 0 && y < CoverSize)
                    {
                        ctx.Fill(Color.FromRgb(200, 100, 255), new EllipsePolygon(x, y, 3, 3));
                    }
                }
                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawArtisticPattern(IImageProcessingContext ctx, Random rng, Rgba32[] palette, int size, string genre)
        {
            var style = rng.Next(9);
            var options = new DrawingOptions();

            switch (style)
            {
                case 0:
                    for (int i = 0; i < 12; i++)
                    {
                        var x = rng.Next(size);
                        var y = rng.Next(size);
                        var w = rng.Next(60, 200);
                        var h = rng.Next(60, 200);
                        var alpha = (byte)rng.Next(80, 200);
                        ctx.Fill(options, Color.FromRgba((byte)palette[rng.Next(palette.Length)].R, (byte)palette[rng.Next(palette.Length)].G, (byte)palette[rng.Next(palette.Length)].B, alpha), new EllipsePolygon(x, y, w, h));
                    }
                    break;

                case 1:
                    for (int i = 0; i < 8; i++)
                    {
                        var points = new PointF[rng.Next(3, 6)];
                        for (int j = 0; j < points.Length; j++)
                        {
                            points[j] = new PointF(rng.Next(size), rng.Next(size));
                        }
                        var alpha = (byte)rng.Next(100, 220);
                        ctx.Fill(options, Color.FromRgba((byte)palette[rng.Next(palette.Length)].R, (byte)palette[rng.Next(palette.Length)].G, (byte)palette[rng.Next(palette.Length)].B, alpha), new Polygon(points));
                    }
                    break;

                case 2:
                    var centerX = size / 2;
                    var centerY = size / 2;
                    for (int i = 0; i < 6; i++)
                    {
                        var radius = rng.Next(40, 200);
                        var alpha = (byte)rng.Next(100, 200);
                        ctx.Draw(options, Color.FromRgba((byte)palette[i % palette.Length].R, (byte)palette[i % palette.Length].G, (byte)palette[i % palette.Length].B, alpha), rng.Next(4, 12), new EllipsePolygon(centerX, centerY, radius, radius));
                    }
                    break;

                case 3:
                    var cellSize = rng.Next(30, 60);
                    for (int x = 0; x < size; x += cellSize)
                    {
                        for (int y = 0; y < size; y += cellSize)
                        {
                            if (rng.NextDouble() > 0.3)
                            {
                                var alpha = (byte)rng.Next(120, 240);
                                ctx.Fill(options, Color.FromRgba((byte)palette[rng.Next(palette.Length)].R, (byte)palette[rng.Next(palette.Length)].G, (byte)palette[rng.Next(palette.Length)].B, alpha), new RectangleF(x, y, cellSize - 4, cellSize - 4));
                            }
                        }
                    }
                    break;

                case 4:
                    var cx = size / 2;
                    var cy = size / 2;
                    var rays = rng.Next(8, 16);
                    for (int i = 0; i < rays; i++)
                    {
                        var angle = (float)(i * Math.PI * 2 / rays);
                        var length = rng.Next(150, 300);
                        var p1 = new PointF(cx, cy);
                        var p2 = new PointF(cx + MathF.Cos(angle) * length, cy + MathF.Sin(angle) * length);
                        var p3 = new PointF(cx + MathF.Cos(angle + 0.1f) * length, cy + MathF.Sin(angle + 0.1f) * length);
                        ctx.Fill(options, Color.FromRgba((byte)palette[i % palette.Length].R, (byte)palette[i % palette.Length].G, (byte)palette[i % palette.Length].B, 180), new Polygon(new PointF[] { p1, p2, p3 }));
                    }
                    break;

                case 5:
                    for (int i = 0; i < 5; i++)
                    {
                        var yBase = rng.Next(size);
                        var amplitude = rng.Next(30, 80);
                        var frequency = rng.Next(2, 5);
                        var pointCount = size / 20 + 1;
                        var points = new PointF[pointCount + 2];
                        points[0] = new PointF(0, yBase);
                        for (int x = 0; x <= size; x += 20)
                        {
                            var idx = x / 20 + 1;
                            points[idx] = new PointF(x, yBase + MathF.Sin(x * frequency / (float)size * MathF.PI * 2) * amplitude);
                        }
                        points[pointCount] = new PointF(size, size);
                        points[pointCount + 1] = new PointF(0, size);
                        ctx.Fill(options, Color.FromRgba((byte)palette[rng.Next(palette.Length)].R, (byte)palette[rng.Next(palette.Length)].G, (byte)palette[rng.Next(palette.Length)].B, 140), new Polygon(points));
                    }
                    break;

                case 6:
                    var topColor = palette[0];
                    var bottomColor = palette[1];

                    ctx.Fill(topColor);
                    ctx.Fill(options, Color.FromRgba(bottomColor.R, bottomColor.G, bottomColor.B, 150), new RectangleF(0, size / 2, size, size / 2));

                    var sunY = rng.Next(50, size / 3);
                    var sunRadius = rng.Next(30, 60);
                    ctx.Fill(Color.FromRgba(255, 200, 100, 200), new EllipsePolygon(size / 2, sunY, sunRadius, sunRadius));

                    for (int i = 0; i < 3; i++)
                    {
                        var waveY = size / 2 + rng.Next(20, 100);
                        var points = new List<PointF>();
                        for (int x = 0; x <= size; x += 20)
                        {
                            points.Add(new PointF(x, waveY + MathF.Sin(x * 0.05f) * 10));
                        }
                        points.Add(new PointF(size, size));
                        points.Add(new PointF(0, size));
                        ctx.Fill(options, Color.FromRgba(255, 255, 255, (byte)rng.Next(30, 80)), new Polygon(points.ToArray()));
                    }
                    break;

                case 7:
                    ctx.Fill(palette[2]);

                    var figureColor = Color.FromRgba(0, 0, 0, 200);
                    var headY = size * 0.25f;
                    var headSize = size * 0.15f;
                    var bodyY = size * 0.4f;

                    ctx.Fill(figureColor, new EllipsePolygon(size / 2, headY, headSize, headSize));

                    var bodyTopW = headSize * 1.5f;
                    var bodyBottomW = headSize * 4f;
                    var bodyH = size * 0.6f;

                    var bodyPoints = new PointF[]
                    {
                        new PointF(size / 2 - bodyTopW / 2, bodyY),
                        new PointF(size / 2 + bodyTopW / 2, bodyY),
                        new PointF(size / 2 + bodyBottomW / 2, bodyY + bodyH),
                        new PointF(size / 2 - bodyBottomW / 2, bodyY + bodyH)
                    };
                    ctx.Fill(figureColor, new Polygon(bodyPoints));
                    break;

                case 8:
                    var words = GenreWords.ContainsKey(genre) ? GenreWords[genre] : GenreWords["default"];
                    var word = words[rng.Next(words.Length)];

                    for (int i = 0; i < 10; i++)
                    {
                        var x = rng.Next(size);
                        var y = rng.Next(size);
                        var fontSize = rng.Next(20, 60);
                        var alpha = (byte)rng.Next(40, 120);
                        var color = palette[rng.Next(palette.Length)];

                        try
                        {
                            var font = SystemFonts.Families.FirstOrDefault().CreateFont(fontSize, FontStyle.Bold);
                            ctx.DrawText(word, font, Color.FromRgba(color.R, color.G, color.B, alpha), new PointF(x, y));
                        }
                        catch { }
                    }
                    for (int i = 0; i < 50; i++)
                    {
                        ctx.Fill(Color.FromRgba((byte)palette[0].R, (byte)palette[0].G, (byte)palette[0].B, (byte)rng.Next(100, 200)), new EllipsePolygon(rng.Next(size), rng.Next(size), 4, 4));
                    }
                    break;
            }
        }

        private void DrawTextWithBackdrop(IImageProcessingContext ctx, string title, string artist, int size)
        {
            try
            {
                var families = SystemFonts.Families.ToList();
                if (families.Count == 0) return;

                var family = families.FirstOrDefault(f => f.Name.Contains("Arial") || f.Name.Contains("Segoe") || f.Name.Contains("Times"));
                if (string.IsNullOrEmpty(family.Name))
                {
                    family = families[0];
                }

                var titleFont = family.CreateFont(26, FontStyle.Bold);
                var artistFont = family.CreateFont(18, FontStyle.Regular);

                var yPosTitle = size * 0.65f;
                var yPosArtist = size * 0.82f;
                var textPadding = 20;

                ctx.DrawText(title, titleFont, Color.FromRgba(0, 0, 0, 200), new PointF(textPadding + 2, yPosTitle + 2));
                ctx.DrawText(title, titleFont, Color.White, new PointF(textPadding, yPosTitle));
                ctx.DrawText(artist, artistFont, Color.FromRgba(0, 0, 0, 180), new PointF(textPadding + 1, yPosArtist + 1));
                ctx.DrawText(artist, artistFont, Color.FromRgba(220, 220, 220, 255), new PointF(textPadding, yPosArtist));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cover] Font error: {ex.Message}");
            }
        }

        private void DrawSeaWaves(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var r = (byte)(0 + 30 * t);
                    var g = (byte)(80 + 100 * t);
                    var b = (byte)(180 + 50 * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }

                for (int i = 0; i < 6; i++)
                {
                    var waveY = CoverSize * 0.3f + i * 40;
                    var amplitude = 20 - i * 2;
                    var points = new List<PointF>();
                    for (int x = 0; x <= CoverSize; x += 15)
                    {
                        points.Add(new PointF(x, waveY + MathF.Sin(x * 0.02f + i) * amplitude));
                    }
                    points.Add(new PointF(CoverSize, CoverSize));
                    points.Add(new PointF(0, CoverSize));
                    var alpha = (byte)(200 - i * 20);
                    ctx.Fill(Color.FromRgba((byte)(100 + i * 20), (byte)180, (byte)255, alpha), new Polygon(points.ToArray()));
                }

                for (int i = 0; i < 8; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize / 3, CoverSize);
                    var radius = rng.Next(15, 35);
                    var alpha = (byte)rng.Next(100, 180);
                    ctx.Fill(Color.FromRgba(150, 210, 255, alpha), new EllipsePolygon(x, y, radius, radius / 2));
                }

                for (int i = 0; i < 20; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize / 2, CoverSize);
                    var size = rng.Next(2, 6);
                    ctx.Fill(Color.White, new EllipsePolygon(x, y, size, size));
                }

                var boatX = CoverSize - 80;
                var boatY = CoverSize * 0.35f;
                var boatPoints = new PointF[]
                {
            new(boatX - 20, boatY),
            new(boatX + 20, boatY),
            new(boatX, boatY - 15)
                };
                ctx.Fill(Color.FromRgb(80, 60, 40), new Polygon(boatPoints));

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }

        private void DrawImpressionistCity(Image<Rgba32> image, Random rng, string title, string artist)
        {
            image.Mutate(ctx =>
            {
                for (int y = 0; y < CoverSize; y++)
                {
                    var t = y / (float)CoverSize;
                    var r = (byte)(100 + 120 * t);
                    var g = (byte)(150 + 80 * t);
                    var b = (byte)(200 + 40 * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(0, y, CoverSize, 1));
                }

                for (int i = 0; i < 60; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var size = rng.Next(8, 25);
                    var r = (byte)rng.Next(180, 255);
                    var g = (byte)rng.Next(150, 220);
                    var b = (byte)rng.Next(80, 180);
                    var alpha = (byte)rng.Next(60, 130);
                    ctx.Fill(Color.FromRgba(r, g, b, alpha), new EllipsePolygon(x, y, size, size));
                }

                for (int i = 0; i < 15; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var width = rng.Next(15, 50);
                    var height = rng.Next(60, 150);
                    var r = (byte)rng.Next(120, 200);
                    var g = (byte)rng.Next(100, 160);
                    var b = (byte)rng.Next(80, 140);
                    ctx.Fill(Color.FromRgb(r, g, b), new RectangleF(x, y, width, height));
                }

                for (int i = 0; i < 8; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize / 3);
                    var radius = rng.Next(20, 45);
                    var r = (byte)rng.Next(200, 255);
                    var g = (byte)rng.Next(180, 240);
                    var b = (byte)rng.Next(100, 180);
                    ctx.Fill(Color.FromRgb(r, g, b), new EllipsePolygon(x, y, radius, radius));
                }

                for (int i = 0; i < 200; i++)
                {
                    var x = rng.Next(CoverSize);
                    var y = rng.Next(CoverSize);
                    var size = rng.Next(2, 5);
                    var r = (byte)rng.Next(200, 255);
                    var g = (byte)rng.Next(180, 240);
                    var b = (byte)rng.Next(100, 200);
                    ctx.Fill(Color.FromRgb(r, g, b), new EllipsePolygon(x, y, size, size));
                }

                DrawTextWithBackdrop(ctx, title, artist, CoverSize);
            });
        }
    }
}