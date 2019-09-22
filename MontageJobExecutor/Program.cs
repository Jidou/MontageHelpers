using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace MontageJobExecutor {
    public class Program {

        public const string BasePath = "Files";


        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }


        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .ConfigureKestrel((context, options) => {
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(180);
                    options.Limits.MaxRequestBodySize = null;
                })
                .ConfigureLogging((hostingContext, logging) => {
                    // Requires `using Microsoft.Extensions.Logging;`
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                })
                .UseStartup<Startup>();


        public static string GetDirectory(string jobId) {
            var directory = $"{Environment.CurrentDirectory}/{BasePath}/{jobId}";
            return directory;
        }
    }
}
