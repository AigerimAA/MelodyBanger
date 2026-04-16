
using MelodyBanger.Services;

namespace MelodyBanger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<LocalizationService>();
            builder.Services.AddScoped<SongGeneratorService>();
            builder.Services.AddSingleton<CoverGeneratorService>();
            builder.Services.AddSingleton<MusicGeneratorService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            var app = builder.Build();
            
            app.UseCors("AllowAll");
            app.UseStaticFiles();            
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
