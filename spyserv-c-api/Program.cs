namespace spyserv_c_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();

            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                });
            });

            var app = builder.Build();
            
            var originalOut = Console.Out;
            using var suppressedOut = new StringWriter();
            Console.SetOut(suppressedOut);

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapControllers();
            app.Run();
            Console.SetOut(originalOut);

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
        }
    }
}