using spyserv_c_api.Services;

namespace spyserv_c_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                EnvironmentName = Environments.Production
            });

            builder.Logging.ClearProviders();
           
            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                });
            });

            var app = builder.Build();
         
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var originalOut = Console.Out;
            using var suppressedOut = new StringWriter();
            Console.SetOut(suppressedOut);
            Console.SetOut(originalOut);

            app.UseHttpsRedirection();
            app.UseAuthorization();

            
            app.MapControllers();
            app.Run();

            #region Port Check
        }
    }
}