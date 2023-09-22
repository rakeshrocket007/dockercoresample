using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace CarvedRock.Api
{
    public class Program
    {
        private static IConfiguration _config;
        public static int Main(string[] args)
        {

            

            try
            {
                var name = typeof(Program).Assembly.GetName().Name;
                _config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                    .AddJsonFile("appsettings.json", false)
                    .AddEnvironmentVariables()
                    .Build();
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Assembly", name)
                    //.WriteTo.Seq(serverUrl: "http://host.docker.internal:5341")
                    .CreateLogger();

                Log.Information("Starting web host");
                var connectionString = _config.GetConnectionString("db");
                var simpleProperty = _config.GetValue<String>("SimpleProperty");
                var nestedProp = "here we go";

                Log.ForContext("ConnectionString", connectionString)
                    .ForContext("SimpleProperty", simpleProperty)
                    .ForContext("Inventory:NestedProperty", nestedProp)
                    .Information("Loaded configuration! {0}, {1}", connectionString, simpleProperty);
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
		    // http://bit.ly/aspnet-builder-defaults
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
