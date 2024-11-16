using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace spyserv_c_api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            #region Port Check
            ////Generic implementation
            //var app = builder.Build();


            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}


            //var lifetime = app.Lifetime;
            //var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            //var logger = loggerFactory.CreateLogger("Startup");

            //lifetime.ApplicationStarted.Register(() =>
            //{
            //    var server = app.Services.GetRequiredService<IServer>();
            //    var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();

            //    if (serverAddressesFeature == null) return;

            //    foreach (var address in serverAddressesFeature.Addresses)
            //    {
            //        logger.LogInformation($"Application is running on: {address}");
            //    }
            //});

            //app.Run();
            #endregion

            builder.Logging.ClearProviders();

            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                });
            });

            var app = builder.Build();

            var lifetime = app.Lifetime;
            lifetime.ApplicationStarted.Register(() =>
            {
                var server = app.Services.GetRequiredService<IServer>();
                var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();

                if (serverAddressesFeature == null) return;

                foreach (var address in serverAddressesFeature.Addresses)
                {
                    Console.WriteLine($"Application is running on: {address}");
                }
            });

            var originalOut = Console.Out;
            using var suppressedOut = new StringWriter();
            Console.SetOut(suppressedOut);

            app.UseHttpsRedirection();
            app.UseAuthorization();

            var summaries = new[]
            {
                    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
                };

            app.MapGet("/weatherforecast", () =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast");

            app.Run();
            Console.SetOut(originalOut);
        }
    }
}